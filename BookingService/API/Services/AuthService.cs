using API.Entities;
using API.Interfaces;
using API.Repositories;

namespace API.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService
) : IAuthService
{
    public async Task<string> Authenticate(string username, string password)
    {
        var user = await userRepository.GetByUsernameAsync(username);
        if (user is null)
        {
            var passwordHash = passwordHasher.Hash(password);
            user = new User(username, passwordHash);
        
            await userRepository.Add(user);
        }
        else if (!passwordHasher.Verify(password, user.PasswordHash))
        {
            throw new Exception("Incorrect password or username.");
        }
        
        var token = jwtService.GenerateToken(user.Id);
        return token;
    }
}