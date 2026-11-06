using API.Entities;
using API.Exceptions;
using API.Interfaces;
using API.Repositories;

namespace API.Services;

public class ResourceService(IResourceRepository resourceRepository, ICurrentUserService currentUserService) : IResourceService
{
    public async Task<IReadOnlyList<BookableResource>> GetAllResources()
    {
        return await resourceRepository.GetAllAsync();
    }
    
    public async Task<BookableResource> GetResource(Guid resourceId)
    {
        var resource = await resourceRepository.GetByIdAsync(resourceId);
        if (resource is null)
            throw new NotFoundException($"Resource with ID {resourceId} was not found.");

        return resource;
    }

    public async Task<Guid> CreateResource(string name, int capacity)
    {
        ValidateResourceInput(name, capacity);

        var resource = new BookableResource(currentUserService.UserId, name.Trim(), capacity);
        await resourceRepository.AddAsync(resource);
        return resource.Id;
    }
    
    public async Task UpdateResource(Guid resourceId, string name, int capacity)
    {
        ValidateResourceInput(name, capacity);

        var resource = await resourceRepository.GetByIdAsync(resourceId);
        if (resource is null)
            throw new NotFoundException($"Resource with ID {resourceId} was not found.");
        
        if (resource.OwnerId != currentUserService.UserId)
            throw new ForbiddenException("You are not authorized to update this resource.");

        resource.Update(name.Trim(), capacity);
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
    
    private static void ValidateResourceInput(string name, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BadRequestException("Resource name cannot be empty.");

        if (capacity <= 0)
            throw new BadRequestException("Capacity must be greater than zero.");
    }
}