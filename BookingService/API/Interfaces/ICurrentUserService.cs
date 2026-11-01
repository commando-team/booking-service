namespace API.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
}