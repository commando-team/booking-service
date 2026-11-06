using MongoDB.Bson.Serialization.Attributes;

namespace API.Entities;

public class BookableResource
{
    [BsonId]
    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    
    public string Name { get; private set; }
    public int Capacity { get; private set; }

    [BsonConstructor]
    public BookableResource(Guid id, Guid ownerId, string name, int capacity)
    {
        Id = id;
        OwnerId = ownerId;
        Name = name;
        Capacity = capacity;
    }
    
    public BookableResource(Guid ownerId, string name, int capacity) :  this(Guid.NewGuid(), ownerId, name, capacity) {}
    
    public void Update(string name, int capacity)
    {
        Name = name;
        Capacity = capacity;
    }
}