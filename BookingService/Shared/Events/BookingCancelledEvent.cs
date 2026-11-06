namespace Shared.Events;

public record BookingCancelledEvent(
    Guid BookingId,
    Guid ResourceId,
    Guid UserId,
    DateTimeOffset CancelledAt
);