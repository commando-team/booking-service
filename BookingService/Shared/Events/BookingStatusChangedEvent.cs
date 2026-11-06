using Shared.Enums;

namespace Shared.Events;

public record BookingStatusChangedEvent(
    Guid BookingId,
    Guid ResourceId,
    Guid UserId,
    BookingStatus Status,
    DateTimeOffset ChangedAt
);