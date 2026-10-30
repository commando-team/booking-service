namespace API.Interfaces;

public interface IAuthService
{
    Task<string> Authenticate(string username, string password);
}