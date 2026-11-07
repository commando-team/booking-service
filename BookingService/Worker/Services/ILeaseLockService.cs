namespace Worker.Services;

public interface ILeaseLockService
{
    Task<bool> TryAcquireAsync(string lockKey, TimeSpan duration, CancellationToken ct = default);
    Task ReleaseAsync(string lockKey, CancellationToken ct = default);
}