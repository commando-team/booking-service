using API.Entities;
using Shared.Enums;

namespace API.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid bookingId);
    
    Task AddAsync(Booking booking);
    
    Task<Booking?> GetByTimeSpanAsync(Guid resourceId, DateTimeOffset from, DateTimeOffset to);
    
    Task<IReadOnlyList<Booking>> GetActiveBookingsByResourceIdsAsync(
        IEnumerable<Guid> resourceIds,
        DateTimeOffset from,
        DateTimeOffset to
    );

    Task UpdateStatusAsync(Guid bookingId, BookingStatus status);
    
    Task DeleteAsync(Guid bookingId);
}