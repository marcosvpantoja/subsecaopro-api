using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubseOpro.Api.Services;
using SubseOpro.Application.Mappers;
using SubseOpro.Application.ViewModels.Room;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Enums;
using SubseOpro.Domain.Interfaces;

namespace SubseOpro.Api.Controllers;

/// <summary>
/// Endpoints de consulta e administração de salas.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly IRepositoryFactory _factory;
    private readonly ITenantContext _tenantContext;

    public RoomsController(IRepositoryFactory factory, ITenantContext tenantContext)
    {
        _factory = factory;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Lista salas disponíveis para reserva.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/rooms com token válido.
    /// Sucesso esperado: 200 com salas disponíveis. Falha esperada: 401 sem token.
    /// </remarks>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Lista de salas disponíveis.</returns>
    [HttpGet]
    public async Task<ActionResult<List<RoomViewModel>>> GetAll(CancellationToken ct)
    {
        var entities = await _factory.Rooms.GetAvailableAsync(ct);
        return Ok(RoomMapper.ToViewModelList(entities));
    }

    /// <summary>
    /// Lista todas as salas para gestão no backoffice.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/rooms/admin com token Backoffice.
    /// Sucesso esperado: 200 com lista completa. Falha esperada: 401 sem token Backoffice.
    /// </remarks>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Lista completa de salas.</returns>
    [HttpGet("admin")]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<List<RoomViewModel>>> GetAllForBackoffice(CancellationToken ct)
    {
        var entities = await _factory.Rooms.GetAllAsync(ct);
        return Ok(RoomMapper.ToViewModelList(entities));
    }

    /// <summary>
    /// Obtém detalhes de uma sala por ID.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/rooms/{id}.
    /// Sucesso esperado: 200 com dados da sala. Falhas esperadas: 400 GUID inválido, 404 sala não encontrada.
    /// </remarks>
    /// <param name="id">Identificador GUID da sala.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Dados da sala solicitada.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<RoomViewModel>> GetById(string id, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var guid)) return BadRequest();

        var entity = await _factory.Rooms.GetByIdAsync(guid, ct);
        if (entity is null) return NotFound();

        return Ok(RoomMapper.ToViewModel(entity));
    }

    /// <summary>
    /// Retorna horários livres de uma sala em uma data.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/rooms/{id}/available-hours?date=dd/MM/yyyy.
    /// Sucesso esperado: 200 com horários disponíveis. Falhas esperadas: 400 para GUID/data inválidos.
    /// </remarks>
    /// <param name="id">Identificador GUID da sala.</param>
    /// <param name="date">Data no formato dd/MM/yyyy ou ISO reconhecido.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Lista de horários disponíveis.</returns>
    [HttpGet("{id}/available-hours")]
    public async Task<ActionResult<List<string>>> GetAvailableHours(
        string id, [FromQuery] string date, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var guid)) return BadRequest();

        if (!DateOnly.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)
            && !DateOnly.TryParse(date, out parsedDate))
            return BadRequest(new { detail = "Data inválida. Use dd/MM/yyyy." });

        var hours = await _factory.Rooms.GetAvailableHoursAsync(guid, parsedDate, ct);
        return Ok(hours);
    }

    /// <summary>
    /// Cria uma nova sala.
    /// </summary>
    /// <remarks>
    /// Validação manual: POST /api/v1/rooms com token Backoffice e nome da sala.
    /// Sucesso esperado: 201 com sala criada. Falhas esperadas: 400 nome ausente, 401 sem token.
    /// </remarks>
    /// <param name="request">Dados de criação da sala.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Sala criada.</returns>
    [HttpPost]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<RoomViewModel>> Create([FromBody] UpsertRoomRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { detail = "Nome é obrigatório." });

        var entity = new Room
        {
            Id = Guid.NewGuid(),
            SubsectionId = _tenantContext.RequireSubsectionId(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Capacity = request.Capacity,
            Features = request.Features ?? [],
            PricePerHour = request.PricePerHour,
            IsFree = request.PricePerHour <= 0,
            Status = ParseStatus(request.Status),
            Color = request.Color ?? "from-blue-400 to-indigo-500",
        };

        await _factory.Rooms.AddAsync(entity, ct);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, RoomMapper.ToViewModel(entity));
    }

    /// <summary>
    /// Atualiza dados de uma sala.
    /// </summary>
    /// <remarks>
    /// Validação manual: PUT /api/v1/rooms/{id} com token Backoffice e campos a alterar.
    /// Sucesso esperado: 200 com sala atualizada. Falhas esperadas: 400 GUID inválido, 404 não encontrada, 401 sem token.
    /// </remarks>
    /// <param name="id">Identificador GUID da sala.</param>
    /// <param name="request">Dados para atualização.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Sala atualizada.</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<RoomViewModel>> Update(string id, [FromBody] UpsertRoomRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var roomId)) return BadRequest();
        var entity = await _factory.Rooms.GetByIdAsync(roomId, ct);
        if (entity is null) return NotFound();

        entity.Name = string.IsNullOrWhiteSpace(request.Name) ? entity.Name : request.Name.Trim();
        entity.Description = request.Description?.Trim() ?? entity.Description;
        entity.Capacity = request.Capacity <= 0 ? entity.Capacity : request.Capacity;
        entity.Features = request.Features ?? entity.Features;
        entity.PricePerHour = request.PricePerHour;
        entity.IsFree = request.PricePerHour <= 0;
        entity.Status = ParseStatus(request.Status);
        entity.Color = request.Color ?? entity.Color;

        await _factory.Rooms.UpdateAsync(entity, ct);
        return Ok(RoomMapper.ToViewModel(entity));
    }

    /// <summary>
    /// Remove uma sala existente.
    /// </summary>
    /// <remarks>
    /// Validação manual: DELETE /api/v1/rooms/{id} com token Backoffice.
    /// Sucesso esperado: 204. Falhas esperadas: 400 GUID inválido, 404 não encontrada, 401 sem token.
    /// </remarks>
    /// <param name="id">Identificador GUID da sala.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Resultado sem conteúdo quando removida.</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Backoffice")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var roomId)) return BadRequest();
        var entity = await _factory.Rooms.GetByIdAsync(roomId, ct);
        if (entity is null) return NotFound();

        await _factory.Rooms.DeleteAsync(entity, ct);
        return NoContent();
    }

    private static RoomStatus ParseStatus(string? status)
    {
        return status?.ToLowerInvariant() switch
        {
            "occupied" => RoomStatus.Occupied,
            "request" => RoomStatus.Request,
            _ => RoomStatus.Available,
        };
    }
}

public class UpsertRoomRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string>? Features { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerHour { get; set; }
    public string? Status { get; set; }
    public string? Color { get; set; }
}
