using API.Entities;

namespace API.Repositories;

public interface IResourceRepository
{
    Task<(IReadOnlyList<BookableResource> Items, long TotalCount)> GetPagedAsync(
        string? search,
        Guid? ownerId,
        int page,
        int pageSize
    );
    
    Task<BookableResource?> GetByIdAsync(Guid id);
    
    Task AddAsync(BookableResource resource);
    
    Task UpdateAsync(BookableResource resource);
    
    Task DeleteAsync(Guid id);
}