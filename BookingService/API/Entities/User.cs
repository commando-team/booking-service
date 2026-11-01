using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Entities;

public class User
{
    [BsonId]
    public Guid Id { get; private set; }
    
    public string Username { get; private set; }
    public string PasswordHash {get; private set;}

    [BsonConstructor]
    public User(Guid id, string username, string passwordHash)
    {
        Id = id;
        Username = username;
        PasswordHash = passwordHash;
    }
    
    public User(string username, string passwordHash) : this(Guid.NewGuid(), username, passwordHash) {}
}