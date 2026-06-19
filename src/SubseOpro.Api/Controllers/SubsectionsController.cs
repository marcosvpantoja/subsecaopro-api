using Microsoft.AspNetCore.Mvc;
using SubseOpro.Application.Mappers;
using SubseOpro.Application.ViewModels.Subsection;
using SubseOpro.Domain.Interfaces;

namespace SubseOpro.Api.Controllers;

[ApiController]
[Route("api/v1/subsections")]
public class SubsectionsController : ControllerBase
{
    private readonly IRepositoryFactory _factory;

    public SubsectionsController(IRepositoryFactory factory)
    {
        _factory = factory;
    }

    [HttpGet("resolve")]
    public async Task<ActionResult<SubsectionResolveViewModel>> Resolve([FromQuery] string slug, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return BadRequest(new { detail = "Parâmetro slug é obrigatório." });

        var entity = await _factory.Subsections.GetBySlugAsync(slug.Trim().ToLowerInvariant(), ct);
        if (entity is null) return NotFound(new { detail = "Subseção não encontrada." });

        return Ok(SubsectionMapper.ToResolveViewModel(entity));
    }
}
