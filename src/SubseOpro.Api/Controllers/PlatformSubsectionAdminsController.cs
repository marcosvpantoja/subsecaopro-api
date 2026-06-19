using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubseOpro.Application.Mappers;
using SubseOpro.Application.Services;
using SubseOpro.Application.ViewModels.Subsection;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Enums;
using SubseOpro.Domain.Interfaces;
using SubseOpro.Infrastructure.Persistence;

namespace SubseOpro.Api.Controllers;

[ApiController]
[Route("api/v1/platform/subsections/{subsectionId:guid}/admins")]
[Authorize(Policy = "PlatformAdmin")]
public class PlatformSubsectionAdminsController : ControllerBase
{
    private readonly IRepositoryFactory _factory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAdminWelcomeEmailService _welcomeEmailService;
    private readonly AppDbContext _db;

    public PlatformSubsectionAdminsController(
        IRepositoryFactory factory,
        IPasswordHasher passwordHasher,
        IAdminWelcomeEmailService welcomeEmailService,
        AppDbContext db)
    {
        _factory = factory;
        _passwordHasher = passwordHasher;
        _welcomeEmailService = welcomeEmailService;
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PlatformSubsectionAdminViewModel>>> List(
        Guid subsectionId, CancellationToken ct)
    {
        if (!await SubsectionExistsAsync(subsectionId, ct)) return NotFound();

        var admins = await _factory.AdminUsers.GetBySubsectionIdAsync(subsectionId, ct);
        return Ok(admins.Select(ToViewModel).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<CreatePlatformSubsectionAdminResultViewModel>> Create(
        Guid subsectionId,
        [FromBody] CreatePlatformSubsectionAdminViewModel request,
        CancellationToken ct)
    {
        if (!await SubsectionExistsAsync(subsectionId, ct)) return NotFound();

        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { detail = "Nome, e-mail e senha são obrigatórios." });
        }

        if (!PasswordPolicy.IsValid(request.Password, out var policyError))
            return BadRequest(new { detail = policyError });

        var role = NormalizeRole(request.Role);
        if (role is null) return BadRequest(new { detail = "Perfil inválido." });

        if (await _factory.AdminUsers.EmailExistsGloballyAsync(request.Email.Trim(), ct: ct))
            return Conflict(new { detail = "E-mail já está em uso por outro administrador." });

        var entity = new AdminUser
        {
            Id = Guid.NewGuid(),
            SubsectionId = subsectionId,
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = role,
            IsActive = request.IsActive,
        };

        await _db.AdminUsers.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);

        var subsection = await _factory.Subsections.GetByIdAsync(subsectionId, ct);
        var welcomeEmailSent = subsection is not null
            && await _welcomeEmailService.TrySendWelcomeAsync(entity, subsection, request.Password, ct);

        var result = new CreatePlatformSubsectionAdminResultViewModel
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            Email = entity.Email,
            Role = ToFriendlyRole(entity.Role),
            IsActive = entity.IsActive,
            WelcomeEmailSent = welcomeEmailSent,
        };

        return CreatedAtAction(nameof(List), new { subsectionId }, result);
    }

    [HttpPatch("{adminId:guid}")]
    public async Task<ActionResult<PlatformSubsectionAdminViewModel>> Update(
        Guid subsectionId,
        Guid adminId,
        [FromBody] UpdatePlatformSubsectionAdminViewModel request,
        CancellationToken ct)
    {
        if (!await SubsectionExistsAsync(subsectionId, ct)) return NotFound();

        var entity = await _factory.AdminUsers.GetByIdForSubsectionAsync(adminId, subsectionId, ct);
        if (entity is null) return NotFound();

        if (request.Name is not null) entity.Name = request.Name.Trim();

        if (request.Email is not null)
        {
            var email = request.Email.Trim();
            if (await _factory.AdminUsers.EmailExistsGloballyAsync(email, entity.Id, ct))
                return Conflict(new { detail = "E-mail já está em uso." });
            entity.Email = email;
        }

        if (request.Role is not null)
        {
            var role = NormalizeRole(request.Role);
            if (role is null) return BadRequest(new { detail = "Perfil inválido." });
            entity.Role = role;
        }

        if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            if (!PasswordPolicy.IsValid(request.Password, out var policyError))
                return BadRequest(new { detail = policyError });
            entity.PasswordHash = _passwordHasher.Hash(request.Password);
        }

        _db.AdminUsers.Update(entity);
        await _db.SaveChangesAsync(ct);

        return Ok(ToViewModel(entity));
    }

    [HttpDelete("{adminId:guid}")]
    public async Task<IActionResult> Delete(Guid subsectionId, Guid adminId, CancellationToken ct)
    {
        if (!await SubsectionExistsAsync(subsectionId, ct)) return NotFound();

        var entity = await _factory.AdminUsers.GetByIdForSubsectionAsync(adminId, subsectionId, ct);
        if (entity is null) return NotFound();

        _db.AdminUsers.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task<bool> SubsectionExistsAsync(Guid id, CancellationToken ct)
    {
        return await _factory.Subsections.GetByIdAsync(id, ct) is not null;
    }

    private static PlatformSubsectionAdminViewModel ToViewModel(AdminUser entity) =>
        new()
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            Email = entity.Email,
            Role = ToFriendlyRole(entity.Role),
            IsActive = entity.IsActive,
        };

    private static string? NormalizeRole(string? role) => role?.Trim().ToLowerInvariant() switch
    {
        "administrador" => "admin",
        "admin" => "admin",
        "financeiro" => "financeiro",
        "comunicação" => "comunicacao",
        "comunicacao" => "comunicacao",
        "secretaria" => "secretaria",
        "moderador" => "moderator",
        "moderator" => "moderator",
        _ => null,
    };

    private static string ToFriendlyRole(string role) => role.Trim().ToLowerInvariant() switch
    {
        "admin" => "Administrador",
        "financeiro" => "Financeiro",
        "comunicacao" => "Comunicação",
        "secretaria" => "Secretaria",
        "moderator" => "Moderador",
        _ => "Administrador",
    };
}
