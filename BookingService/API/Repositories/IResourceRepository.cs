using API.Entities;

namespace API.Repositories;

public interface IResourceRepository
{
    Task<BookableResource?> GetByIdAsync(Guid id);
    
    Task AddAsync(BookableResource resource);
    
    Task DeleteAsync(Guid id);
}