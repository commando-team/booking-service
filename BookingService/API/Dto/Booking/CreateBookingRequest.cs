namespace API.Dto.Booking;

public record CreateBookingRequest(Guid ResourceId, DateTimeOffset From, DateTimeOffset To);