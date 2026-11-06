using System.Text.RegularExpressions;
using API.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace API.Repositories;

public class ResourceRepository(IMongoDatabase db) : IResourceRepository
{
    private const string CollectionName = "resources";
    private readonly IMongoCollection<BookableResource> _collection = db.GetCollection<BookableResource>(CollectionName);

    public async Task<(IReadOnlyList<BookableResource> Items, long TotalCount)> GetPagedAsync(
        string? search,
        Guid? ownerId,
        int page,
        int pageSize)
    {
        var builder = Builders<BookableResource>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(search))
        {
            filter &= builder.Regex(r => r.Name, new BsonRegularExpression(Regex.Escape(search.Trim()), "i"));
        }

        if (ownerId.HasValue)
        {
            filter &= builder.Eq(r => r.OwnerId, ownerId.Value);
        }

        var totalCount = await _collection.CountDocumentsAsync(filter);
        var items = await _collection.Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return (items, totalCount);
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