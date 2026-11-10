using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Enums;

namespace Worker.Entities;

public class Booking
{
    [BsonId]
    public Guid Id { get; set; }
    
    public Guid ResourceId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset From { get; set; }
    public DateTimeOffset To { get; set; }
    
    [BsonRepresentation(BsonType.String)]
    public BookingStatus Status { get; set; }
}