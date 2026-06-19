using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubseOpro.Api.Services;
using SubseOpro.Application.Mappers;
using SubseOpro.Application.ViewModels.Platform;

namespace SubseOpro.Api.Controllers;

[ApiController]
[Route("api/v1/platform/app-config")]
public class PlatformAppSettingsController : ControllerBase
{
    private readonly HubContentService _hub;

    public PlatformAppSettingsController(HubContentService hub)
    {
        _hub = hub;
    }

    [HttpGet]
    [Authorize(Policy = "PlatformAdmin")]
    public async Task<ActionResult<PlatformAppSettingsViewModel>> Get(CancellationToken ct)
    {
        return Ok(await _hub.BuildConfigViewModelAsync(ct));
    }

    [HttpPatch]
    [Authorize(Policy = "PlatformAdmin")]
    public async Task<ActionResult<PlatformAppSettingsViewModel>> Update(
        [FromBody] UpdatePlatformAppSettingsViewModel request,
        CancellationToken ct)
    {
        var entity = await _hub.GetOrCreateSettingsAsync(ct);
        PlatformAppSettingsMapper.ApplyUpdate(entity, request);
        await _hub.SaveSettingsAsync(entity, ct);
        return Ok(await _hub.BuildConfigViewModelAsync(ct));
    }
}
