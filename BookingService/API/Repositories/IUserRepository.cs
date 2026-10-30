using API.Entities;

namespace API.Repositories;

public interface IUserRepository
{
    Task Add(User user);
    
    Task<User?> GetByUsernameAsync(string username);
}