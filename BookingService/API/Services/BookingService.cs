using API.Entities;
using API.Interfaces;
using API.Repositories;

namespace API.Services;

public class BookingService(IBookingRepository bookingRepository, ICurrentUserService currentUserService) : IBookingService
{
    public async Task<Booking> GetBooking(Guid bookingId)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
            throw new Exception("Booking not found");
        return booking;
    }

    public async Task<Guid> CreateBooking(Guid resourceId, DateTimeOffset from, DateTimeOffset to)
    {
        var overlappingBooking = await bookingRepository.GetByTimeSpanAsync(resourceId, from, to);
        if (overlappingBooking is not null)
            throw new Exception("Booking time overlap");
        
        var booking = new Booking(resourceId, currentUserService.UserId, from, to);
        await bookingRepository.AddAsync(booking);
        
        return booking.Id;
    }

    public async Task ConfirmBooking(Guid bookingId)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
            throw new Exception("Booking not found");
        
        if (booking.UserId != currentUserService.UserId)
            throw new Exception("You are not authorized to confirm this booking");
        
        await bookingRepository.ConfirmAsync(bookingId);
    }

    public async Task CancelBooking(Guid bookingId)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
            throw new Exception("Booking not found");
        
        if (booking.UserId != currentUserService.UserId)
            throw new Exception("You are not authorized to cancel this booking");
        
        await bookingRepository.DeleteAsync(booking.Id);
    }
}