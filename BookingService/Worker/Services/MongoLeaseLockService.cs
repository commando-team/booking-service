using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Worker.Services;

public class DistributedLock
{
    [BsonId]
    public string Id { get; set; } = default!;
    public string HolderId { get; set; } = default!;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset AcquiredAt { get; set; }
}

public class MongoLeaseLockService : ILeaseLockService
{
    private readonly IMongoCollection<DistributedLock> _locks;
    private readonly string _workerInstanceId = Guid.NewGuid().ToString("N")[..8];
    private readonly ILogger<MongoLeaseLockService> _logger;

    public MongoLeaseLockService(IMongoDatabase database, ILogger<MongoLeaseLockService> logger)
    {
        _locks = database.GetCollection<DistributedLock>("distributed_locks");
        _logger = logger;
    }

    public async Task<bool> TryAcquireAsync(string lockKey, TimeSpan duration, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.Add(duration);

        try
        {
            await _locks.UpdateOneAsync(
                x => x.Id == lockKey,
                Builders<DistributedLock>.Update.SetOnInsert(x => x.ExpiresAt, DateTimeOffset.MinValue),
                new UpdateOptions { IsUpsert = true },
                ct);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
        }

        var filter = Builders<DistributedLock>.Filter.And(
            Builders<DistributedLock>.Filter.Eq(x => x.Id, lockKey),
            Builders<DistributedLock>.Filter.Lt(x => x.ExpiresAt, now)
        );

        var update = Builders<DistributedLock>.Update
            .Set(x => x.HolderId, _workerInstanceId)
            .Set(x => x.AcquiredAt, now)
            .Set(x => x.ExpiresAt, expiresAt);

        var result = await _locks.UpdateOneAsync(filter, update, cancellationToken: ct);
        var acquired = result.ModifiedCount > 0;

        if (acquired)
        {
            _logger.LogInformation("Instance {InstanceId} acquired lease '{LockKey}' until {ExpiresAt:O}",
                _workerInstanceId, lockKey, expiresAt);
        }

        return acquired;
    }

    public async Task ReleaseAsync(string lockKey, CancellationToken ct = default)
    {
        var filter = Builders<DistributedLock>.Filter.And(
            Builders<DistributedLock>.Filter.Eq(x => x.Id, lockKey),
            Builders<DistributedLock>.Filter.Eq(x => x.HolderId, _workerInstanceId)
        );

        var update = Builders<DistributedLock>.Update.Set(x => x.ExpiresAt, DateTimeOffset.MinValue);
        await _locks.UpdateOneAsync(filter, update, cancellationToken: ct);

        _logger.LogInformation("Instance {InstanceId} released lease '{LockKey}'", _workerInstanceId, lockKey);
    }
}   