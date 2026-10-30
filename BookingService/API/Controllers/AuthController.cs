using API.Dto.Auth;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<string>> Authenticate([FromBody] AuthRequest request)
    {
        var token = await authService.Authenticate(request.Username, request.Password);
        return Ok(token);
    }
}