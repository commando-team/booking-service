using MongoDB.Bson.Serialization.Attributes;

namespace API.Entities;

public class Booking
{
    [BsonId]
    public Guid Id { get; private set; }
    
    public Guid ResourceId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTimeOffset From { get; private set; }
    public DateTimeOffset To { get; private set; }
    public bool Confirmed { get; private set; }

    [BsonConstructor]
    public Booking(Guid id, Guid resourceId, Guid userId, DateTimeOffset from, DateTimeOffset to, bool confirmed = false)
    {
        Id = id;
        ResourceId = resourceId;
        UserId = userId;
        From = from;
        To = to;
        Confirmed = confirmed;
    }
    
    public Booking(Guid resourceId, Guid userId, DateTimeOffset from, DateTimeOffset to)
        : this(Guid.NewGuid(), resourceId, userId, from, to) {}
}