namespace API.Dto.Booking;

public record GetBookingResponse(Guid ResourceId, DateTimeOffset From, DateTimeOffset To, bool Confirmed);