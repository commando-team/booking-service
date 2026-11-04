using API.Entities;
using API.Interfaces;
using API.Repositories;

namespace API.Services;

public class ResourceService(IResourceRepository resourceRepository, ICurrentUserService currentUserService) : IResourceService
{
    public async Task<BookableResource> GetResource(Guid resourceId)
    {
        var resource = await resourceRepository.GetByIdAsync(resourceId);
        if (resource is null)
            throw new Exception($"Resource not found");
        return resource;
    }

    public async Task<Guid> CreateResource(string name, int capacity)
    {
        var resource = new BookableResource(currentUserService.UserId, name, capacity);
        await resourceRepository.AddAsync(resource);
        return resource.Id;
    }

    public async Task DeleteResource(Guid resourceId)
    {
        var resource = await resourceRepository.GetByIdAsync(resourceId);
        if (resource is null)
            throw new Exception("Resource not found");
        
        if (resource.OwnerId != currentUserService.UserId)
            throw new Exception("You are not authorized to delete this resource");
        
        await resourceRepository.DeleteAsync(resourceId);
    }
}