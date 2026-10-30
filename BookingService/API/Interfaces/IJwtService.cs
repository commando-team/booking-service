namespace API.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid userId);
}