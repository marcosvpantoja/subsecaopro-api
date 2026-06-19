using SubseOpro.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubseOpro.Application.Mappers;
using SubseOpro.Application.ViewModels.Election;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Interfaces;
using SubseOpro.Infrastructure.Persistence;

namespace SubseOpro.Api.Controllers;

/// <summary>
/// Consulta e administração de eleições e candidatos.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class ElectionsController : ControllerBase
{
    private readonly IRepositoryFactory _factory;
    private readonly AppDbContext _context;
    private readonly ITenantContext _tenantContext;

    public ElectionsController(IRepositoryFactory factory, AppDbContext context, ITenantContext tenantContext)
    {
        _factory = factory;
        _context = context;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Obtém a eleição ativa no momento.
    /// </summary>
    /// <remarks>
    /// Validação manual: execute GET /api/v1/elections/current sem autenticação.
    /// Sucesso esperado: 200 com eleição atual. Falha esperada: 404 quando não há eleição ativa.
    /// </remarks>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Eleição ativa para consumo público.</returns>
    [HttpGet("current")]
    [AllowAnonymous]
    public async Task<ActionResult<ElectionViewModel>> GetCurrent(CancellationToken ct)
    {
        var entity = await _factory.Elections.GetCurrentAsync(ct);
        if (entity is null) return NotFound();

        return Ok(ElectionMapper.ToViewModel(entity));
    }

    /// <summary>
    /// Lista eleições para gestão no backoffice.
    /// </summary>
    /// <remarks>
    /// Validação manual: execute GET /api/v1/elections/admin com token Backoffice.
    /// Sucesso esperado: 200 com eleições e candidatos. Falha esperada: 401 sem autenticação.
    /// </remarks>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Lista completa para administração.</returns>
    [HttpGet("admin")]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<List<BackofficeElectionDto>>> GetAllForBackoffice(CancellationToken ct)
    {
        var elections = await _context.Elections
            .Include(e => e.Candidates.OrderBy(c => c.Name))
            .OrderByDescending(e => e.Year)
            .AsNoTracking()
            .ToListAsync(ct);

        return Ok(elections.Select(ToBackofficeDto).ToList());
    }

    /// <summary>
    /// Cria uma nova eleição.
    /// </summary>
    /// <remarks>
    /// Validação manual: POST /api/v1/elections com título e datas.
    /// Sucesso esperado: 200 com a eleição criada. Falhas esperadas: 400 payload inválido, 401 sem token Backoffice.
    /// </remarks>
    /// <param name="request">Dados de criação da eleição.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Eleição criada.</returns>
    [HttpPost]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<BackofficeElectionDto>> Create(
        [FromBody] UpsertElectionAdminRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { detail = "Título é obrigatório." });

        var entity = new Election
        {
            Id = Guid.NewGuid(),
            SubsectionId = _tenantContext.RequireSubsectionId(),
            Year = ParseDate(request.StartDate).Year,
            Term = request.Title.Trim(),
            EndDate = ParseDateOnly(request.EndDate),
            PortalUrl = string.IsNullOrWhiteSpace(request.PortalUrl) ? "#" : request.PortalUrl.Trim(),
            IsActive = string.Equals(request.Status, "Aberta", StringComparison.OrdinalIgnoreCase),
        };

        await _context.Elections.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        return Ok(ToBackofficeDto(entity));
    }

    /// <summary>
    /// Atualiza uma eleição existente.
    /// </summary>
    /// <remarks>
    /// Validação manual: PUT /api/v1/elections/{id} com dados atualizados e token Backoffice.
    /// Sucesso esperado: 200 com dados atualizados. Falhas esperadas: 400 ID inválido, 404 não encontrada, 401 sem token.
    /// </remarks>
    /// <param name="id">Identificador GUID da eleição.</param>
    /// <param name="request">Dados para atualização.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Eleição atualizada.</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<BackofficeElectionDto>> Update(
        string id,
        [FromBody] UpsertElectionAdminRequest request,
        CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var electionId)) return BadRequest();

        var entity = await _context.Elections
            .Include(e => e.Candidates)
            .FirstOrDefaultAsync(e => e.Id == electionId, ct);

        if (entity is null) return NotFound();

        entity.Term = string.IsNullOrWhiteSpace(request.Title) ? entity.Term : request.Title.Trim();
        entity.Year = ParseDate(request.StartDate).Year;
        entity.EndDate = ParseDateOnly(request.EndDate);
        entity.PortalUrl = string.IsNullOrWhiteSpace(request.PortalUrl) ? entity.PortalUrl : request.PortalUrl.Trim();
        entity.IsActive = string.Equals(request.Status, "Aberta", StringComparison.OrdinalIgnoreCase);

        await _context.SaveChangesAsync(ct);
        return Ok(ToBackofficeDto(entity));
    }

    /// <summary>
    /// Exclui uma eleição pelo identificador.
    /// </summary>
    /// <remarks>
    /// Validação manual: DELETE /api/v1/elections/{id} com token Backoffice.
    /// Sucesso esperado: 204. Falhas esperadas: 400 ID inválido, 404 não encontrada, 401 sem token.
    /// </remarks>
    /// <param name="id">Identificador GUID da eleição.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Resultado sem conteúdo quando removida.</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Backoffice")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var electionId)) return BadRequest();
        var entity = await _context.Elections.FirstOrDefaultAsync(e => e.Id == electionId, ct);
        if (entity is null) return NotFound();
        _context.Elections.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>
    /// Adiciona candidato a uma eleição.
    /// </summary>
    /// <remarks>
    /// Validação manual: POST /api/v1/elections/{id}/candidates com nome, OAB e função.
    /// Sucesso esperado: 200 com eleição atualizada. Falhas esperadas: 400 ID inválido, 404 eleição inexistente, 401 sem token.
    /// </remarks>
    /// <param name="id">Identificador GUID da eleição.</param>
    /// <param name="request">Dados do candidato.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Eleição com a lista de candidatos atualizada.</returns>
    [HttpPost("{id}/candidates")]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<BackofficeElectionDto>> AddCandidate(
        string id,
        [FromBody] AddElectionCandidateRequest request,
        CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var electionId)) return BadRequest();

        var entity = await _context.Elections
            .Include(e => e.Candidates)
            .FirstOrDefaultAsync(e => e.Id == electionId, ct);
        if (entity is null) return NotFound();

        var candidate = new Candidate
        {
            Id = Guid.NewGuid(),
            ElectionId = electionId,
            Name = request.Name.Trim(),
            Number = request.Oab.Trim(),
            Slate = request.Role.Trim(),
            Bio = request.Role.Trim(),
            ColorStart = "from-blue-500",
            ColorEnd = "to-indigo-600",
        };

        entity.Candidates.Add(candidate);
        await _context.SaveChangesAsync(ct);
        return Ok(ToBackofficeDto(entity));
    }

    private static BackofficeElectionDto ToBackofficeDto(Election entity)
    {
        return new BackofficeElectionDto
        {
            Id = entity.Id.ToString(),
            Title = entity.Term,
            Description = "Eleição oficial da subseção.",
            Status = entity.IsActive ? "Aberta" : "Encerrada",
            StartDate = $"{entity.Year}-01-01",
            EndDate = entity.EndDate.ToString("yyyy-MM-dd"),
            TotalVoters = 450,
            Candidates = entity.Candidates.Select(c => new BackofficeCandidateDto
            {
                Id = c.Id.ToString(),
                Name = c.Name,
                Oab = c.Number,
                Role = string.IsNullOrWhiteSpace(c.Slate) ? "Candidato" : c.Slate,
                Votes = 0,
            }).ToList(),
            CreatedAt = DateTime.UtcNow.ToString("O"),
        };
    }

    private static DateTime ParseDate(string? value)
    {
        if (DateTime.TryParse(value, out var parsed)) return parsed;
        return DateTime.UtcNow;
    }

    private static DateOnly ParseDateOnly(string? value)
    {
        if (DateOnly.TryParse(value, out var parsed)) return parsed;
        return DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
    }
}

public class UpsertElectionAdminRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public int? TotalVoters { get; set; }
    public string? PortalUrl { get; set; }
}

public class AddElectionCandidateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Oab { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class BackofficeElectionDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Rascunho";
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public int TotalVoters { get; set; }
    public List<BackofficeCandidateDto> Candidates { get; set; } = [];
    public string CreatedAt { get; set; } = string.Empty;
}

public class BackofficeCandidateDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Oab { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int Votes { get; set; }
}
