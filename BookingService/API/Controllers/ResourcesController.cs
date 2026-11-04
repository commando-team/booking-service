using API.Dto.Resource;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ResourcesController(IResourceService resourceService) : ControllerBase
{
    [HttpGet("{id:guid}", Name = nameof(GetResourceById))]
    public async Task<ActionResult<GetResourceResponse>> GetResourceById(Guid id)
    {
        var resource = await resourceService.GetResource(id);
        return Ok(new GetResourceResponse(resource.Name, resource.Capacity));
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateResource([FromBody] CreateResourceRequest request)
    {
        var id = await resourceService.CreateResource(request.Name, request.Capacity);
        return CreatedAtRoute(nameof(GetResourceById), new {id = id}, id);
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await resourceService.DeleteResource(id);
        return NoContent();
    }
}