using Shared.Enums;

namespace API.Dto.Booking;

public record GetBookingResponse(
    Guid Id,
    Guid ResourceId,
    Guid UserId,
    DateTimeOffset From,
    DateTimeOffset To,
    BookingStatus Status,
    bool Confirmed
);