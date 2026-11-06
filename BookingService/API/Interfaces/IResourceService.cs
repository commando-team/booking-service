using API.Entities;

namespace API.Interfaces;

public interface IResourceService
{
    Task<IReadOnlyList<BookableResource>> GetAllResources();
    
    Task<BookableResource> GetResource(Guid resourceId);
    
    Task<Guid> CreateResource(string name, int capacity);
    
    Task UpdateResource(Guid resourceId, string name, int capacity);
    
    Task DeleteResource(Guid resourceId);
}