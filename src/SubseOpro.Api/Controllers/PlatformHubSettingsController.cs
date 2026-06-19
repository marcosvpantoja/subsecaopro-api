using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubseOpro.Application.Mappers;
using SubseOpro.Application.ViewModels.Platform;
using SubseOpro.Domain;
using SubseOpro.Infrastructure.Persistence;

namespace SubseOpro.Api.Controllers;

[ApiController]
[Route("api/v1/platform/hub-branding")]
public class PlatformHubSettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PlatformHubSettingsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PlatformHubSettingsViewModel>> Get(CancellationToken ct)
    {
        var entity = await GetOrCreateAsync(ct);
        return Ok(PlatformHubSettingsMapper.ToViewModel(entity));
    }

    [HttpPatch]
    [Authorize(Policy = "PlatformAdmin")]
    public async Task<ActionResult<PlatformHubSettingsViewModel>> Update(
        [FromBody] UpdatePlatformHubSettingsViewModel request,
        CancellationToken ct)
    {
        var entity = await GetOrCreateAsync(ct);
        PlatformHubSettingsMapper.ApplyUpdate(entity, request);
        _db.PlatformHubSettings.Update(entity);
        await _db.SaveChangesAsync(ct);
        return Ok(PlatformHubSettingsMapper.ToViewModel(entity));
    }

    private async Task<Domain.Entities.PlatformHubSettings> GetOrCreateAsync(CancellationToken ct)
    {
        var entity = await _db.PlatformHubSettings
            .FirstOrDefaultAsync(x => x.Id == PlatformDefaults.HubSettingsId, ct);

        if (entity is not null) return entity;

        entity = PlatformHubSettingsMapper.CreateDefault();
        await _db.PlatformHubSettings.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
        return entity;
    }
}
