using API.Entities;
using MongoDB.Driver;
using Shared.Enums;

namespace API.Repositories;

public class BookingRepository(IMongoDatabase db) : IBookingRepository
{
    private const string CollectionName = "bookings";
    private readonly IMongoCollection<Booking> _collection = db.GetCollection<Booking>(CollectionName);
    
    public async Task<Booking?> GetByIdAsync(Guid bookingId)
    {
        return await _collection.Find(b => b.Id == bookingId).FirstOrDefaultAsync();
    }

    public async Task AddAsync(Booking booking)
    {
        await _collection.InsertOneAsync(booking);
    }

    public async Task<Booking?> GetByTimeSpanAsync(Guid resourceId, DateTimeOffset from, DateTimeOffset to)
    {
        return await _collection.Find(b => 
            b.ResourceId == resourceId &&
            b.Status != BookingStatus.Cancelled &&
            b.Status != BookingStatus.AutoCancelled &&
            b.From < to && 
            b.To > from
        ).FirstOrDefaultAsync();
    }
    
    public async Task<IReadOnlyList<Booking>> GetActiveBookingsByResourceIdsAsync(
        IEnumerable<Guid> resourceIds,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        var builder = Builders<Booking>.Filter;
        var filter = builder.And(
            builder.In(b => b.ResourceId, resourceIds),
            builder.Ne(b => b.Status, BookingStatus.Cancelled),
            builder.Ne(b => b.Status, BookingStatus.AutoCancelled),
            builder.Lt(b => b.From, to),
            builder.Gt(b => b.To, from)
        );

        return await _collection.Find(filter)
            .SortBy(b => b.From)
            .ToListAsync();
    }

    public async Task UpdateStatusAsync(Guid bookingId, BookingStatus status)
    {
        var update = Builders<Booking>.Update.Set(b => b.Status, status);
        await _collection.UpdateOneAsync(b => b.Id == bookingId, update);
    }

    public async Task DeleteAsync(Guid bookingId)
    {
        await _collection.DeleteOneAsync(b => b.Id == bookingId);
    }
}