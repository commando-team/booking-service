using API.Entities;

namespace API.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid bookingId);
    
    Task AddAsync(Booking booking);
    
    Task<Booking?> GetByTimeSpanAsync(Guid resourceId, DateTimeOffset from, DateTimeOffset to);

    Task ConfirmAsync(Guid bookingId);
    
    Task DeleteAsync(Guid bookingId);
}