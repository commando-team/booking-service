using API.Entities;
using MongoDB.Driver;

namespace API.Repositories;

public class UserRepository(IMongoDatabase db) : IUserRepository
{
    private const string CollectionName = "users";
    private readonly IMongoCollection<User> _collection = db.GetCollection<User>(CollectionName);

    public async Task AddAsync(User user)
    {
        await _collection.InsertOneAsync(user);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _collection.Find(x => x.Username == username).FirstOrDefaultAsync();
    }
}