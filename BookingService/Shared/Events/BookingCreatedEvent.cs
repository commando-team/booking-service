namespace Shared.Events;

public record BookingCreatedEvent(
    Guid BookingId,
    Guid ResourceId,
    Guid UserId,
    DateTimeOffset From,
    DateTimeOffset To,
    DateTimeOffset CreatedAt
);