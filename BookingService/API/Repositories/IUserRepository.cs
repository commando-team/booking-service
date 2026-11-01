using API.Entities;

namespace API.Repositories;

public interface IUserRepository
{
    Task AddAsync(User user);
    
    Task<User?> GetByUsernameAsync(string username);
}