using API.Dto.Common;
using API.Dto.Resource;

namespace API.Interfaces;

public interface IResourceService
{
    Task<PagedResult<GetResourceResponse>> GetResources(GetResourcesFilterQuery filter);
    
    Task<GetResourceResponse> GetResource(Guid resourceId);
    
    Task<Guid> CreateResource(string name);
    
    Task UpdateResource(Guid resourceId, string name);
    
    Task DeleteResource(Guid resourceId);
}