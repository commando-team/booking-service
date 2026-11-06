using API.Dto.Common;
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
    [HttpGet]
    public async Task<ActionResult<PagedResult<GetResourceResponse>>> GetAll([FromQuery] GetResourcesFilterQuery filter)
    {
        var result = await resourceService.GetResources(filter);
        return Ok(result);
    }
    
    [HttpGet("{id:guid}", Name = nameof(GetResourceById))]
    public async Task<ActionResult<GetResourceResponse>> GetResourceById(Guid id)
    {
        var resource = await resourceService.GetResource(id);
        return Ok(resource);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateResource([FromBody] CreateResourceRequest request)
    {
        var id = await resourceService.CreateResource(request.Name);
        return CreatedAtRoute(nameof(GetResourceById), new { id }, id);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateResource(Guid id, [FromBody] UpdateResourceRequest request)
    {
        await resourceService.UpdateResource(id, request.Name);
        return NoContent();
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await resourceService.DeleteResource(id);
        return NoContent();
    }
}