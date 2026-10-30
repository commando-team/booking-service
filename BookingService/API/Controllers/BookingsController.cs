using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    [HttpPost]
    public async Task Create()
    {
        // create booking
        throw new NotImplementedException();
    }
    
    [HttpGet("{id:guid}")]
    public async Task Get(Guid id)
    {
        // get booking status
        throw new NotImplementedException();
    }
    
    [HttpPost("{id:guid}")]
    public async Task Confirm(Guid id)
    {
        // confirm your booking
        throw new NotImplementedException();
    }
    
    [HttpDelete("{id:guid}")]
    public async Task Cancel(Guid id)
    {
        // cancel your booking
        throw new NotImplementedException();
    }
}