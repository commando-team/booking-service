using API.Entities;

namespace API.Interfaces;

public interface IResourceService
{
    Task<BookableResource> GetResource(Guid resourceId);
    
    Task<Guid> CreateResource(string name, int capacity);
    
    Task DeleteResource(Guid resourceId);
}