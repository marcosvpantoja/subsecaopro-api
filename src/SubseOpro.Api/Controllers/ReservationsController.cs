using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubseOpro.Application.Mappers;
using SubseOpro.Application.ViewModels.Reservation;
using SubseOpro.Domain.Interfaces;

namespace SubseOpro.Api.Controllers;

/// <summary>
/// Endpoints para reservas de salas.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IRepositoryFactory _factory;

    public ReservationsController(IRepositoryFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Lista reservas do advogado autenticado.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/reservations/me?paidOnly=false com token de advogado.
    /// Sucesso esperado: 200 com reservas do usuário. Falha esperada: 401 sem token.
    /// </remarks>
    /// <param name="paidOnly">Quando verdadeiro, retorna apenas reservas pagas.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Lista de reservas do advogado logado.</returns>
    [HttpGet("me")]
    public async Task<ActionResult<List<ReservationViewModel>>> GetMyReservations(
        [FromQuery] bool paidOnly, CancellationToken ct)
    {
        var lawyerId = GetLawyerId();
        if (lawyerId is null) return Unauthorized();

        var entities = paidOnly
            ? await _factory.Reservations.GetPaidByLawyerIdAsync(lawyerId.Value, ct)
            : await _factory.Reservations.GetByLawyerIdAsync(lawyerId.Value, ct);

        return Ok(ReservationMapper.ToViewModelList(entities));
    }

    /// <summary>
    /// Lista todas as reservas para administração.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/reservations/admin com token Backoffice.
    /// Sucesso esperado: 200 com reservas. Falha esperada: 401 sem token Backoffice.
    /// </remarks>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Lista completa de reservas.</returns>
    [HttpGet("admin")]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<List<ReservationViewModel>>> GetAllForBackoffice(CancellationToken ct)
    {
        var entities = await _factory.Reservations.GetAllAsync(ct);
        return Ok(ReservationMapper.ToViewModelList(entities));
    }

    /// <summary>
    /// Cria uma nova reserva para o advogado autenticado.
    /// </summary>
    /// <remarks>
    /// Validação manual: POST /api/v1/reservations com roomId, date, startTime e endTime válidos.
    /// Sucesso esperado: 200 com reserva criada. Falhas esperadas: 400 dados inválidos, 401 sem token.
    /// </remarks>
    /// <param name="request">Dados da reserva.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Reserva criada.</returns>
    [HttpPost]
    public async Task<ActionResult<ReservationViewModel>> Create(
        [FromBody] CreateReservationViewModel request, CancellationToken ct)
    {
        var lawyerId = GetLawyerId();
        if (lawyerId is null) return Unauthorized();

        if (!Guid.TryParse(request.RoomId, out var roomId)) return BadRequest();

        if (!DateOnly.TryParseExact(request.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            && !DateOnly.TryParse(request.Date, out date))
            return BadRequest(new { detail = "Data inválida." });

        if (!TimeOnly.TryParse(request.StartTime, out var start)
            || !TimeOnly.TryParse(request.EndTime, out var end))
            return BadRequest(new { detail = "Horário inválido." });

        var entity = await _factory.Reservations.CreateReservationAsync(
            lawyerId.Value, roomId, date, start, end, ct);

        return Ok(ReservationMapper.ToViewModel(entity));
    }

    /// <summary>
    /// Cancela uma reserva do advogado autenticado.
    /// </summary>
    /// <remarks>
    /// Validação manual: PATCH /api/v1/reservations/{id}/cancel com token de advogado.
    /// Sucesso esperado: 200 com reserva cancelada. Falhas esperadas: 400 GUID inválido, 401 sem token.
    /// </remarks>
    /// <param name="id">Identificador GUID da reserva.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Reserva após cancelamento.</returns>
    [HttpPatch("{id}/cancel")]
    public async Task<ActionResult<ReservationViewModel>> Cancel(string id, CancellationToken ct)
    {
        var lawyerId = GetLawyerId();
        if (lawyerId is null) return Unauthorized();

        if (!Guid.TryParse(id, out var reservationId)) return BadRequest();

        var entity = await _factory.Reservations.CancelReservationAsync(
            reservationId, lawyerId.Value, ct);

        return Ok(ReservationMapper.ToViewModel(entity));
    }

    private Guid? GetLawyerId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
