using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Entities;

public class User
{
    [BsonElement]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; private set; }
    [BsonElement]
    public string Username { get; private set; }
    [BsonElement]
    public string PasswordHash {get; private set;}

    [BsonConstructor]
    public User(Guid id, string username, string passwordHash)
    {
        Id = id;
        Username = username;
        PasswordHash = passwordHash;
    }
    
    public User(string username, string passwordHash)
    {
        Id = Guid.NewGuid();
        Username = username;
        PasswordHash = passwordHash;
    }
}