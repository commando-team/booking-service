namespace API.Dto.Resource;

public record BookedTimeSlotResponse(
    Guid BookingId,
    DateTimeOffset From,
    DateTimeOffset To
);