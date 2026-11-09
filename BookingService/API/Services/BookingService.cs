using API.Entities;
using API.Exceptions;
using API.Interfaces;
using API.Repositories;
using API.Telemetry;
using MassTransit;
using Shared.Enums;
using Shared.Events;

namespace API.Services;

public class BookingService(
    IBookingRepository bookingRepository,
    IResourceRepository resourceRepository,
    ICurrentUserService currentUserService,
    IPublishEndpoint publishEndpoint
) : IBookingService
{
    public async Task<Booking> GetBooking(Guid bookingId)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
            throw new NotFoundException($"Booking with ID {bookingId} was not found.");

        return booking;
    }

    public async Task<Guid> CreateBooking(Guid resourceId, DateTimeOffset from, DateTimeOffset to)
    {
        if (from >= to)
            throw new BadRequestException("Booking start time must be earlier than end time.");

        if (from < DateTimeOffset.UtcNow)
            throw new BadRequestException("Cannot create a booking in the past.");

        var resource = await resourceRepository.GetByIdAsync(resourceId);
        if (resource is null)
            throw new NotFoundException($"Resource with ID {resourceId} was not found.");

        var overlappingBooking = await bookingRepository.GetByTimeSpanAsync(resourceId, from, to);
        if (overlappingBooking is not null)
        {
            BookingMetrics.BookingConflict();
            throw new ConflictException("The selected time slot is already booked.");
        }

        var booking = new Booking(resourceId, currentUserService.UserId, from, to);
        await bookingRepository.AddAsync(booking);
        BookingMetrics.BookingCreated();

        await publishEndpoint.Publish(new BookingCreatedEvent(
            booking.Id,
            booking.ResourceId,
            booking.UserId,
            booking.From,
            booking.To,
            DateTimeOffset.UtcNow)
        );
        
        return booking.Id;
    }

    public async Task ConfirmBooking(Guid bookingId)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
            throw new NotFoundException($"Booking with ID {bookingId} was not found.");
        
        if (booking.UserId != currentUserService.UserId)
            throw new ForbiddenException("You are not authorized to confirm this booking.");

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.AutoCancelled)
            throw new BadRequestException("Cannot confirm a cancelled booking.");

        if (booking.Status == BookingStatus.Confirmed)
            return;
        
        await bookingRepository.UpdateStatusAsync(bookingId, BookingStatus.Confirmed);
        BookingMetrics.BookingConfirmed();

        await publishEndpoint.Publish(new BookingStatusChangedEvent(
            booking.Id,
            booking.ResourceId,
            booking.UserId,
            BookingStatus.Confirmed,
            DateTimeOffset.UtcNow)
        );
    }

    public async Task CancelBooking(Guid bookingId)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
            throw new NotFoundException($"Booking with ID {bookingId} was not found.");
        
        if (booking.UserId != currentUserService.UserId)
            throw new ForbiddenException("You are not authorized to cancel this booking.");

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.AutoCancelled)
            return;
        
        await bookingRepository.UpdateStatusAsync(booking.Id, BookingStatus.Cancelled);
        BookingMetrics.BookingCancelledByUser();

        var cancelledAt = DateTimeOffset.UtcNow;

        await publishEndpoint.Publish(new BookingCancelledEvent(
            booking.Id,
            booking.ResourceId,
            booking.UserId,
            cancelledAt)
        );

        await publishEndpoint.Publish(new BookingStatusChangedEvent(
            booking.Id,
            booking.ResourceId,
            booking.UserId,
            BookingStatus.Cancelled,
            cancelledAt)
        );
    }
}