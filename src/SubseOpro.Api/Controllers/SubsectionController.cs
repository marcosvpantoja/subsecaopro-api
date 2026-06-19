using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubseOpro.Application.Mappers;
using SubseOpro.Application.ViewModels.Subsection;
using SubseOpro.Domain.Interfaces;

namespace SubseOpro.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SubsectionController : ControllerBase
{
    private readonly IRepositoryFactory _factory;

    public SubsectionController(IRepositoryFactory factory)
    {
        _factory = factory;
    }

    [HttpGet("info")]
    public async Task<ActionResult<SubsectionInfoViewModel>> GetInfo(CancellationToken ct)
    {
        var entity = await _factory.Subsections.GetCurrentAsync(ct);
        if (entity is null) return NotFound();

        var social = await _factory.SocialAccounts.GetOrderedAsync(ct);
        return Ok(SubsectionMapper.ToInfoViewModel(entity, social));
    }

    [HttpPatch("info")]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<SubsectionInfoViewModel>> UpdateInfo(
        [FromBody] UpdateSubsectionSettingsViewModel request,
        CancellationToken ct)
    {
        var entity = await _factory.Subsections.GetCurrentAsync(ct);
        if (entity is null) return NotFound();

        SubsectionMapper.ApplySettings(entity, request);
        await _factory.Subsections.UpdateAsync(entity, ct);
        await _factory.Subsections.SaveChangesAsync(ct);

        var social = await _factory.SocialAccounts.GetOrderedAsync(ct);
        return Ok(SubsectionMapper.ToInfoViewModel(entity, social));
    }
}
