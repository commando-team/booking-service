using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Interfaces;

namespace API.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public Guid UserId => Guid.TryParse(_user?.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId)
        ? userId
        : throw new UnauthorizedAccessException("User is not authenticated.");
}