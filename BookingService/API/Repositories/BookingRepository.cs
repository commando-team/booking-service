using API.Entities;
using MongoDB.Driver;

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
        return await _collection.Find(b => b.ResourceId == resourceId && (b.From < to && b.To > from)).FirstOrDefaultAsync();
    }

    public async Task ConfirmAsync(Guid bookingId)
    {
        var update = Builders<Booking>.Update.Set(b => b.Confirmed, true);
        
        await _collection.UpdateOneAsync(b => b.Id == bookingId, update);
    }

    public async Task DeleteAsync(Guid bookingId)
    {
        await _collection.DeleteOneAsync(b => b.Id == bookingId);
    }
}