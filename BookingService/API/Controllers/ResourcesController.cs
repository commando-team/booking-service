using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ResourcesController : ControllerBase
{
    [HttpPost]
    public async Task Create()
    {
        // create bookable resource
        throw new NotImplementedException();
    }
    
    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id)
    {
        // delete bookable resource
        throw new NotImplementedException();
    }
}