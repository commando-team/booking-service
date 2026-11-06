using API.Entities;
using API.Exceptions;
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
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new BadRequestException("Username and password must not be empty.");
        
        var user = await userRepository.GetByUsernameAsync(username);
        if (user is null)
        {
            var passwordHash = passwordHasher.Hash(password);
            user = new User(username, passwordHash);
        
            await userRepository.AddAsync(user);
        }
        else if (!passwordHasher.Verify(password, user.PasswordHash))
        {
            throw new BadRequestException("Incorrect username or password.");
        }
        
        var token = jwtService.GenerateToken(user.Id);
        return token;
    }
}