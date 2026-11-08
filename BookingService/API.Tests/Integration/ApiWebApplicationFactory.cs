using API.Exceptions;
using API.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Tests.Integration;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAuthService>();
            services.AddSingleton<IAuthService, TestAuthService>();
        });
    }
}

internal sealed class TestAuthService : IAuthService
{
    public Task<string> Authenticate(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new BadRequestException("Username and password must not be empty.");

        return Task.FromResult("test-token");
    }
}
