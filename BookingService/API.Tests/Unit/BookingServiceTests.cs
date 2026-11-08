using API.Entities;
using API.Exceptions;
using API.Interfaces;
using API.Repositories;
using API.Services;
using MassTransit;
using Moq;
using NUnit.Framework;
using Shared.Enums;
using Shared.Events;

namespace API.Tests.Unit;

[TestFixture]
public class BookingServiceTests
{
    private Mock<IBookingRepository> _bookings = null!;
    private Mock<IResourceRepository> _resources = null!;
    private Mock<ICurrentUserService> _currentUser = null!;
    private Mock<IPublishEndpoint> _publisher = null!;
    private BookingService _sut = null!;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _resourceId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _bookings = new Mock<IBookingRepository>();
        _resources = new Mock<IResourceRepository>();
        _currentUser = new Mock<ICurrentUserService>();
        _publisher = new Mock<IPublishEndpoint>();
        _currentUser.SetupGet(x => x.UserId).Returns(_userId);

        _publisher
            .Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new BookingService(
            _bookings.Object,
            _resources.Object,
            _currentUser.Object,
            _publisher.Object);
    }

    [Test]
    public void CreateBooking_WhenRangeIsInvalid_ThrowsBadRequest()
    {
        var from = DateTimeOffset.UtcNow.AddHours(2);

        Assert.That(
            async () => await _sut.CreateBooking(_resourceId, from, from),
            Throws.TypeOf<BadRequestException>());

        _resources.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public void CreateBooking_WhenResourceIsMissing_ThrowsNotFound()
    {
        _resources.Setup(x => x.GetByIdAsync(_resourceId)).ReturnsAsync((BookableResource?)null);
        var from = DateTimeOffset.UtcNow.AddHours(2);

        Assert.That(
            async () => await _sut.CreateBooking(_resourceId, from, from.AddHours(1)),
            Throws.TypeOf<NotFoundException>());
    }

    [Test]
    public void CreateBooking_WhenSlotOverlapsActiveBooking_ThrowsConflict()
    {
        _resources.Setup(x => x.GetByIdAsync(_resourceId))
            .ReturnsAsync(new BookableResource(_userId, "Room A"));
        _bookings.Setup(x => x.GetByTimeSpanAsync(
                _resourceId,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(new Booking(
                _resourceId,
                _userId,
                DateTimeOffset.UtcNow.AddHours(2),
                DateTimeOffset.UtcNow.AddHours(3)));

        var from = DateTimeOffset.UtcNow.AddHours(2);

        Assert.That(
            async () => await _sut.CreateBooking(_resourceId, from, from.AddHours(1)),
            Throws.TypeOf<ConflictException>());

        _bookings.Verify(x => x.AddAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Test]
    public void ConfirmBooking_WhenBookingBelongsToAnotherUser_ThrowsForbidden()
    {
        var booking = new Booking(
            _resourceId,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddHours(2),
            DateTimeOffset.UtcNow.AddHours(3));
        _bookings.Setup(x => x.GetByIdAsync(booking.Id)).ReturnsAsync(booking);

        Assert.That(
            async () => await _sut.ConfirmBooking(booking.Id),
            Throws.TypeOf<ForbiddenException>());

        _bookings.Verify(x => x.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<BookingStatus>()), Times.Never);
    }

    [Test]
    public async Task ConfirmBooking_WhenAlreadyConfirmed_IsIdempotent()
    {
        var booking = new Booking(
            Guid.NewGuid(),
            _resourceId,
            _userId,
            DateTimeOffset.UtcNow.AddHours(2),
            DateTimeOffset.UtcNow.AddHours(3),
            BookingStatus.Confirmed);
        _bookings.Setup(x => x.GetByIdAsync(booking.Id)).ReturnsAsync(booking);

        await _sut.ConfirmBooking(booking.Id);

        _bookings.Verify(x => x.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<BookingStatus>()), Times.Never);
        _publisher.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task CancelBooking_WhenOwnedByCurrentUser_ChangesStatus()
    {
        var booking = new Booking(
            _resourceId,
            _userId,
            DateTimeOffset.UtcNow.AddHours(2),
            DateTimeOffset.UtcNow.AddHours(3));
        _bookings.Setup(x => x.GetByIdAsync(booking.Id)).ReturnsAsync(booking);

        await _sut.CancelBooking(booking.Id);

        _bookings.Verify(x => x.UpdateStatusAsync(booking.Id, BookingStatus.Cancelled), Times.Once);
        _publisher.Verify(x => x.Publish(It.IsAny<BookingCancelledEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(x => x.Publish(It.IsAny<BookingStatusChangedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
