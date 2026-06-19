using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubseOpro.Api.Services;
using SubseOpro.Application.Mappers;
using SubseOpro.Application.Services;
using SubseOpro.Application.ViewModels.Lawyer;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Interfaces;

namespace SubseOpro.Api.Controllers;

/// <summary>
/// Consulta e gestão de advogados.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class LawyersController : ControllerBase
{
    private readonly IRepositoryFactory _factory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantContext _tenantContext;

    public LawyersController(
        IRepositoryFactory factory,
        IPasswordHasher passwordHasher,
        ITenantContext tenantContext)
    {
        _factory = factory;
        _passwordHasher = passwordHasher;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Lista advogados públicos com filtros opcionais.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/lawyers usando parâmetros q, specialty, city e online quando necessário.
    /// Sucesso esperado: 200 com lista filtrada.
    /// </remarks>
    /// <param name="q">Texto livre para busca.</param>
    /// <param name="specialty">Especialidade desejada.</param>
    /// <param name="city">Cidade de atuação.</param>
    /// <param name="online">Indica atendimento online.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Lista de advogados.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<LawyerViewModel>>> GetAll(
        [FromQuery] string? q,
        [FromQuery] string? specialty,
        [FromQuery] string? city,
        [FromQuery] bool? online,
        CancellationToken ct)
    {
        var entities = await _factory.Lawyers.SearchAsync(q, specialty, city, online, ct);
        return Ok(LawyerMapper.ToViewModelList(entities));
    }

    /// <summary>
    /// Lista todos os advogados para o backoffice.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/lawyers/admin com token Backoffice.
    /// Sucesso esperado: 200 com lista completa. Falha esperada: 401 sem token.
    /// </remarks>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Lista completa de advogados.</returns>
    [HttpGet("admin")]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<List<LawyerViewModel>>> GetAllForBackoffice(CancellationToken ct)
    {
        var entities = await _factory.Lawyers.GetAllAsync(ct);
        return Ok(LawyerMapper.ToViewModelList(entities));
    }

    /// <summary>
    /// Obtém advogado por identificador.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/lawyers/{id}.
    /// Sucesso esperado: 200 com dados do advogado. Falhas esperadas: 400 GUID inválido, 404 não encontrado.
    /// </remarks>
    /// <param name="id">Identificador GUID do advogado.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Dados do advogado solicitado.</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<LawyerViewModel>> GetById(string id, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var guid)) return BadRequest();

        var entity = await _factory.Lawyers.GetByIdAsync(guid, ct);
        if (entity is null) return NotFound();

        return Ok(LawyerMapper.ToViewModel(entity));
    }

    /// <summary>
    /// Cria advogado via backoffice.
    /// </summary>
    /// <remarks>
    /// Validação manual: POST /api/v1/lawyers com token Backoffice e payload mínimo (name, oab).
    /// Sucesso esperado: 200 com advogado criado. Falhas esperadas: 400 dados obrigatórios, 409 OAB duplicada, 401 sem token.
    /// </remarks>
    /// <param name="request">Dados cadastrais do advogado.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Advogado criado.</returns>
    [HttpPost]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<LawyerViewModel>> CreateByBackoffice(
        [FromBody] CreateLawyerRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Oab))
            return BadRequest(new { detail = "Nome e OAB são obrigatórios." });

        var existing = await _factory.Lawyers.GetByOabNumberAsync(request.Oab.Trim(), ct);
        if (existing is not null)
            return Conflict(new { detail = "Já existe advogado com esta OAB." });

        var name = request.Name.Trim();
        var initials = string.Concat(
            name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Take(2)
                .Select(p => p[0].ToString().ToUpperInvariant()));

        var entity = new Lawyer
        {
            Id = Guid.NewGuid(),
            SubsectionId = _tenantContext.RequireSubsectionId(),
            Name = name,
            OabNumber = request.Oab.Trim(),
            City = request.City?.Trim() ?? string.Empty,
            Bio = request.Bio?.Trim() ?? string.Empty,
            Rating = 5,
            Online = request.Online,
            Whatsapp = request.Whatsapp?.Trim() ?? string.Empty,
            Email = request.Email?.Trim() ?? string.Empty,
            Address = request.Address?.Trim() ?? string.Empty,
            Availability = request.Availability?.Trim() ?? string.Empty,
            Color = string.IsNullOrWhiteSpace(request.Color) ? "from-blue-500 to-indigo-600" : request.Color.Trim(),
            Initials = initials,
            PhotoUrl = request.Photo?.Trim() ?? string.Empty,
            IsActive = request.IsActive,
            IsPublic = true,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = _passwordHasher.Hash(string.IsNullOrWhiteSpace(request.Password) ? "Adv@12345" : request.Password.Trim()),
        };

        await _factory.Lawyers.AddAsync(entity, ct);
        await _factory.Lawyers.UpdateProfileAsync(entity, request.Specialties ?? [], ct);

        var created = await _factory.Lawyers.GetByIdAsync(entity.Id, ct);
        return Ok(LawyerMapper.ToViewModel(created!));
    }

    /// <summary>
    /// Atualiza o perfil do advogado autenticado.
    /// </summary>
    /// <remarks>
    /// Validação manual: PUT /api/v1/lawyers/me com token de advogado e payload de atualização.
    /// Sucesso esperado: 200 com perfil atualizado. Falhas esperadas: 401 sem token, 404 registro inexistente.
    /// </remarks>
    /// <param name="request">Dados de perfil para atualização.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Perfil atualizado.</returns>
    [HttpPut("me")]
    [Authorize]
    public async Task<ActionResult<LawyerViewModel>> UpdateMe(
        [FromBody] LawyerUpdateViewModel request, CancellationToken ct)
    {
        var lawyerId = GetLawyerId();
        if (lawyerId is null) return Unauthorized();

        var entity = await _factory.Lawyers.GetByIdAsync(lawyerId.Value, ct);
        if (entity is null) return NotFound();

        LawyerMapper.UpdateEntity(entity, request);
        await _factory.Lawyers.UpdateProfileAsync(entity, request.Specialties, ct);

        var updated = await _factory.Lawyers.GetByIdAsync(lawyerId.Value, ct);
        return Ok(LawyerMapper.ToViewModel(updated!));
    }

    /// <summary>
    /// Altera status de ativo/inativo de um advogado.
    /// </summary>
    /// <remarks>
    /// Validação manual: PATCH /api/v1/lawyers/{id}/status com token Backoffice e corpo {\"isActive\":true/false}.
    /// Sucesso esperado: 200 com advogado atualizado. Falhas esperadas: 400 GUID inválido, 404 não encontrado, 401 sem token.
    /// </remarks>
    /// <param name="id">Identificador GUID do advogado.</param>
    /// <param name="request">Novo status do advogado.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Advogado com status atualizado.</returns>
    [HttpPatch("{id}/status")]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<LawyerViewModel>> UpdateStatus(string id, [FromBody] LawyerStatusRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var lawyerId)) return BadRequest();
        var entity = await _factory.Lawyers.GetByIdAsync(lawyerId, ct);
        if (entity is null) return NotFound();

        entity.IsActive = request.IsActive;
        await _factory.Lawyers.UpdateAsync(entity, ct);
        return Ok(LawyerMapper.ToViewModel(entity));
    }

    /// <summary>
    /// Atualiza dados de um advogado específico no backoffice.
    /// </summary>
    /// <remarks>
    /// Validação manual: PUT /api/v1/lawyers/{id} com token Backoffice e campos de edição.
    /// Sucesso esperado: 200 com dados atualizados. Falhas esperadas: 400 GUID inválido, 404 não encontrado, 401 sem token.
    /// </remarks>
    /// <param name="id">Identificador GUID do advogado.</param>
    /// <param name="request">Dados para atualização.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Dados do advogado após atualização.</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<LawyerViewModel>> UpdateLawyer(
        string id,
        [FromBody] LawyerUpdateViewModel request,
        CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var lawyerId)) return BadRequest();
        var entity = await _factory.Lawyers.GetByIdAsync(lawyerId, ct);
        if (entity is null) return NotFound();

        LawyerMapper.UpdateEntity(entity, request);
        await _factory.Lawyers.UpdateProfileAsync(entity, request.Specialties, ct);

        var updated = await _factory.Lawyers.GetByIdAsync(lawyerId, ct);
        return Ok(LawyerMapper.ToViewModel(updated!));
    }

    private Guid? GetLawyerId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

public class LawyerStatusRequest
{
    public bool IsActive { get; set; }
}

public class CreateLawyerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Oab { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Bio { get; set; }
    public string? Whatsapp { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Availability { get; set; }
    public string? Photo { get; set; }
    public string? Color { get; set; }
    public bool Online { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Password { get; set; }
    public List<string>? Specialties { get; set; }
}
