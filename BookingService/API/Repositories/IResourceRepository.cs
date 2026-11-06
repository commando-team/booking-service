using API.Entities;

namespace API.Repositories;

public interface IResourceRepository
{
    Task<IReadOnlyList<BookableResource>> GetAllAsync();
    
    Task<BookableResource?> GetByIdAsync(Guid id);
    
    Task AddAsync(BookableResource resource);
    
    Task UpdateAsync(BookableResource resource);
    
    Task DeleteAsync(Guid id);
}