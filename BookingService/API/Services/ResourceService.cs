using API.Dto.Common;
using API.Dto.Resource;
using API.Entities;
using API.Exceptions;
using API.Interfaces;
using API.Repositories;

namespace API.Services;

public class ResourceService(
    IResourceRepository resourceRepository,
    IBookingRepository bookingRepository,
    ICurrentUserService currentUserService
) : IResourceService
{
    public async Task<PagedResult<GetResourceResponse>> GetResources(GetResourcesFilterQuery filter)
    {
        var (resources, totalCount) = await resourceRepository.GetPagedAsync(
            filter.Search,
            filter.OwnerId,
            filter.Page,
            filter.PageSize);

        if (resources.Count == 0)
        {
            return new PagedResult<GetResourceResponse>([], filter.Page, filter.PageSize, totalCount, 0);
        }

        var now = DateTimeOffset.UtcNow;
        var weekAhead = now.AddDays(7);

        var resourceIds = resources.Select(r => r.Id).ToList();
        var activeBookings = await bookingRepository.GetActiveBookingsByResourceIdsAsync(resourceIds, now, weekAhead);

        var bookingsGrouped = activeBookings
            .GroupBy(b => b.ResourceId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(b => new BookedTimeSlotResponse(b.Id, b.From, b.To)).ToList()
            );

        var items = resources.Select(r => new GetResourceResponse(
            r.Id,
            r.Name,
            r.OwnerId,
            bookingsGrouped.TryGetValue(r.Id, out var slots) ? slots : []
        )).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        return new PagedResult<GetResourceResponse>(items, filter.Page, filter.PageSize, totalCount, totalPages);
    }
    
    public async Task<GetResourceResponse> GetResource(Guid resourceId)
    {
        var resource = await resourceRepository.GetByIdAsync(resourceId);
        if (resource is null)
            throw new NotFoundException($"Resource with ID {resourceId} was not found.");

        var now = DateTimeOffset.UtcNow;
        var weekAhead = now.AddDays(7);

        var bookings = await bookingRepository.GetActiveBookingsByResourceIdsAsync([resourceId], now, weekAhead);
        var slots = bookings.Select(b => new BookedTimeSlotResponse(b.Id, b.From, b.To)).ToList();

        return new GetResourceResponse(resource.Id, resource.Name, resource.OwnerId, slots);
    }

    public async Task<Guid> CreateResource(string name)
    {
        ValidateName(name);

        var resource = new BookableResource(currentUserService.UserId, name.Trim());
        await resourceRepository.AddAsync(resource);
        return resource.Id;
    }
    
    public async Task UpdateResource(Guid resourceId, string name)
    {
        ValidateName(name);

        var resource = await resourceRepository.GetByIdAsync(resourceId);
        if (resource is null)
            throw new NotFoundException($"Resource with ID {resourceId} was not found.");
        
        if (resource.OwnerId != currentUserService.UserId)
            throw new ForbiddenException("You are not authorized to update this resource.");

        resource.Update(name.Trim());
        await resourceRepository.UpdateAsync(resource);
    }

    public async Task DeleteResource(Guid resourceId)
    {
        var resource = await resourceRepository.GetByIdAsync(resourceId);
        if (resource is null)
            throw new NotFoundException($"Resource with ID {resourceId} was not found.");
        
        if (resource.OwnerId != currentUserService.UserId)
            throw new ForbiddenException("You are not authorized to delete this resource.");
        
        await resourceRepository.DeleteAsync(resourceId);
    }
    
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BadRequestException("Resource name cannot be empty.");
    }
}