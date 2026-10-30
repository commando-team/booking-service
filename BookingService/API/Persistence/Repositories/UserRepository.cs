using API.Entities;
using API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API.Persistence.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task Add(User user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await context.Users.FirstOrDefaultAsync(user => user.Username == username);
    }
}