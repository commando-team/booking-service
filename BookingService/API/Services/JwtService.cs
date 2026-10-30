using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Interfaces;
using API.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class JwtService(IOptions<JwtOptions> options) : IJwtService
{
    private static readonly JwtSecurityTokenHandler Handler = new();
    
    private readonly JwtOptions _options = options.Value;
    private readonly SigningCredentials _credentials = new(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.Key)),
        SecurityAlgorithms.HmacSha256);
    
    public string GenerateToken(Guid userId)
    {
        var claims = new Claim[] { new(JwtRegisteredClaimNames.Sub, userId.ToString()) };

        var token = new JwtSecurityToken(
            claims: claims,
            signingCredentials: _credentials,
            issuer: _options.Issuer,
            audience: _options.Audience,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes)
        );

        return Handler.WriteToken(token);
    }
}