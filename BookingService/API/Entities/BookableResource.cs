using MongoDB.Bson.Serialization.Attributes;

namespace API.Entities;

public class BookableResource
{
    [BsonId]
    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    
    public string Name { get; private set; }

    [BsonConstructor]
    public BookableResource(Guid id, Guid ownerId, string name)
    {
        Id = id;
        OwnerId = ownerId;
        Name = name;
    }
    
    public BookableResource(Guid ownerId, string name) : this(Guid.NewGuid(), ownerId, name) {}

    public void Update(string name)
    {
        Name = name;
    }
}