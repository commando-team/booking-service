using API.Entities;

namespace API.Interfaces;

public interface IBookingService
{
    Task<Booking> GetBooking(Guid bookingId);
    
    Task<Guid> CreateBooking(Guid resourceId, DateTimeOffset from, DateTimeOffset to);

    Task ConfirmBooking(Guid bookingId);
    
    Task CancelBooking(Guid bookingId);
}