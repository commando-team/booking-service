using MassTransit;
using MongoDB.Driver;
using Shared.Enums;
using Shared.Events;
using Worker.Entities;
using Worker.Services;

namespace Worker.Workers;

public class ExpiredBookingsWorker : BackgroundService
{
    private const string LockKey = "expired-bookings-processor";
    private readonly IMongoCollection<Booking> _bookingsCollection;
    private readonly ILeaseLockService _leaseLock;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExpiredBookingsWorker> _logger;

    public ExpiredBookingsWorker(
        IMongoDatabase database,
        ILeaseLockService leaseLock,
        IPublishEndpoint publishEndpoint,
        IConfiguration configuration,
        ILogger<ExpiredBookingsWorker> logger)
    {
        _bookingsCollection = database.GetCollection<Booking>("bookings");
        _leaseLock = leaseLock;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = _configuration.GetValue("Worker:ScanIntervalSeconds", 30);
        var leaseDurationSeconds = _configuration.GetValue("Worker:LeaseDurationSeconds", 45);
        var expirationMinutes = _configuration.GetValue("Worker:ExpirationMinutes", 15);

        _logger.LogInformation("ExpiredBookingsWorker started. Interval: {Interval}s, Expiration: {Exp}m",
            intervalSeconds, expirationMinutes);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var acquired = await _leaseLock.TryAcquireAsync(
                    LockKey, 
                    TimeSpan.FromSeconds(leaseDurationSeconds), 
                    stoppingToken);

                if (!acquired)
                {
                    continue;
                }

                try
                {
                    await ProcessExpiredBookingsAsync(expirationMinutes, stoppingToken);
                }
                finally
                {
                    await _leaseLock.ReleaseAsync(LockKey, stoppingToken);
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error occurred during expired bookings scan.");
            }
        }
    }

    private async Task ProcessExpiredBookingsAsync(int expirationMinutes, CancellationToken ct)
    {
        var threshold = DateTimeOffset.UtcNow.AddMinutes(-expirationMinutes);

        var filter = Builders<Booking>.Filter.And(
            Builders<Booking>.Filter.Eq(b => b.Status, BookingStatus.Pending),
            Builders<Booking>.Filter.Lte(b => b.From, threshold)
        );

        var expiredBookings = await _bookingsCollection.Find(filter).ToListAsync(ct);

        if (expiredBookings.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Found {Count} expired unconfirmed booking(s).", expiredBookings.Count);

        foreach (var booking in expiredBookings)
        {
            var updateFilter = Builders<Booking>.Filter.And(
                Builders<Booking>.Filter.Eq(b => b.Id, booking.Id),
                Builders<Booking>.Filter.Eq(b => b.Status, BookingStatus.Pending)
            );

            var update = Builders<Booking>.Update.Set(b => b.Status, BookingStatus.AutoCancelled);
            var updateResult = await _bookingsCollection.UpdateOneAsync(updateFilter, update, cancellationToken: ct);

            if (updateResult.ModifiedCount > 0)
            {
                var now = DateTimeOffset.UtcNow;

                await _publishEndpoint.Publish(new BookingStatusChangedEvent(
                    booking.Id,
                    booking.ResourceId,
                    booking.UserId,
                    BookingStatus.AutoCancelled,
                    now), ct);

                await _publishEndpoint.Publish(new BookingCancelledEvent(
                    booking.Id,
                    booking.ResourceId,
                    booking.UserId,
                    now), ct);

                _logger.LogInformation("Booking {BookingId} auto-cancelled. Slot freed.", booking.Id);
            }
        }
    }
}