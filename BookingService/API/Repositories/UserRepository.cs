using API.Entities;
using MongoDB.Driver;

namespace API.Repositories;

public class UserRepository(IMongoDatabase db) : IUserRepository
{
    private readonly IMongoCollection<User> userCollection = db.GetCollection<User>(CollectionName);
    private const string CollectionName = "users";
    
    public async Task Add(User user)
    {
        await userCollection.InsertOneAsync(user);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await userCollection.Find(x => x.Username == username).FirstOrDefaultAsync();
    }
}