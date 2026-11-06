using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Enums;

namespace API.Entities;

public class Booking
{
    [BsonId]
    public Guid Id { get; private set; }
    
    public Guid ResourceId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTimeOffset From { get; private set; }
    public DateTimeOffset To { get; private set; }
    
    [BsonRepresentation(BsonType.String)]
    public BookingStatus Status { get; private set; }

    public bool Confirmed => Status == BookingStatus.Confirmed;

    [BsonConstructor]
    public Booking(Guid id, Guid resourceId, Guid userId, DateTimeOffset from, DateTimeOffset to, BookingStatus status)
    {
        Id = id;
        ResourceId = resourceId;
        UserId = userId;
        From = from;
        To = to;
        Status = status;
    }
    
    public Booking(Guid resourceId, Guid userId, DateTimeOffset from, DateTimeOffset to)
        : this(Guid.NewGuid(), resourceId, userId, from, to, BookingStatus.Pending) {}

    public void Confirm()
    {
        Status = BookingStatus.Confirmed;
    }

    public void Cancel()
    {
        Status = BookingStatus.Cancelled;
    }
}