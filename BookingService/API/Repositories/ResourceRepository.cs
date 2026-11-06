using API.Entities;
using MongoDB.Driver;

namespace API.Repositories;

public class ResourceRepository(IMongoDatabase db) : IResourceRepository
{
    private const string CollectionName = "resources";
    private readonly IMongoCollection<BookableResource> _collection = db.GetCollection<BookableResource>(CollectionName);

    public async Task<IReadOnlyList<BookableResource>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }
    
    public async Task<BookableResource?> GetByIdAsync(Guid id)
    {
        return await _collection.Find(r => r.Id == id).FirstOrDefaultAsync();
    }

    public async Task AddAsync(BookableResource resource)
    {
        await _collection.InsertOneAsync(resource);
    }
    
    public async Task UpdateAsync(BookableResource resource)
    {
        await _collection.ReplaceOneAsync(r => r.Id == resource.Id, resource);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _collection.DeleteOneAsync(r => r.Id == id);
    }
}