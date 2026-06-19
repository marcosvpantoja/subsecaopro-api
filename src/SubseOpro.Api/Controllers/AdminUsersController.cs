using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubseOpro.Api.Services;
using SubseOpro.Application.Services;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Interfaces;

namespace SubseOpro.Api.Controllers;

/// <summary>
/// Gerencia usuários administradores do backoffice.
/// </summary>
[ApiController]
[Route("api/v1/adminusers")]
[Authorize(Policy = "Backoffice")]
public class AdminUsersController : ControllerBase
{
    private readonly IRepositoryFactory _factory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantContext _tenantContext;

    public AdminUsersController(
        IRepositoryFactory factory,
        IPasswordHasher passwordHasher,
        ITenantContext tenantContext)
    {
        _factory = factory;
        _passwordHasher = passwordHasher;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Lista todos os administradores ativos e inativos.
    /// </summary>
    /// <remarks>
    /// Validação manual: autentique com token Backoffice, execute GET /api/v1/adminusers e valide retorno 200 com lista ordenada por nome.
    /// Teste de falha: sem token deve retornar 401.
    /// </remarks>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Lista de administradores com perfil e status.</returns>
    [HttpGet]
    public async Task<ActionResult<List<AdminUserResponse>>> GetAll(CancellationToken ct)
    {
        await EnsureSystemAdminsAsync(ct);

        var entities = await _factory.AdminUsers.GetAllAsync(ct);
        var users = entities
            .OrderBy(a => a.Name)
            .Select(ToResponse)
            .ToList();

        return Ok(users);
    }

    /// <summary>
    /// Cria um novo usuário administrador.
    /// </summary>
    /// <remarks>
    /// Validação manual: envie nome, e-mail e perfil válidos para POST /api/v1/adminusers.
    /// Sucesso esperado: 201 com dados criados. Falhas esperadas: 400 para payload inválido, 409 para e-mail duplicado, 401 sem token.
    /// </remarks>
    /// <param name="request">Dados de criação do administrador.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Administrador criado.</returns>
    [HttpPost]
    public async Task<ActionResult<AdminUserResponse>> Create([FromBody] UpsertAdminUserRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { detail = "Nome e e-mail são obrigatórios." });

        var role = NormalizeRole(request.Role);
        if (role is null)
            return BadRequest(new { detail = "Perfil inválido." });

        var exists = await _factory.AdminUsers.GetByEmailAsync(request.Email.Trim(), ct);
        if (exists is not null)
            return Conflict(new { detail = "Já existe um administrador com este e-mail." });

        var entity = new AdminUser
        {
            Id = Guid.NewGuid(),
            SubsectionId = _tenantContext.RequireSubsectionId(),
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Role = role,
            IsActive = request.IsActive ?? true,
            PasswordHash = _passwordHasher.Hash("Temp@12345"),
        };

        await _factory.AdminUsers.AddAsync(entity, ct);
        return CreatedAtAction(nameof(GetAll), new { id = entity.Id }, ToResponse(entity));
    }

    /// <summary>
    /// Remove um administrador pelo identificador.
    /// </summary>
    /// <remarks>
    /// Validação manual: execute DELETE /api/v1/adminusers/{id} com um ID existente.
    /// Sucesso esperado: 204 sem corpo. Falhas esperadas: 400 para GUID inválido, 404 para registro inexistente, 401 sem token.
    /// </remarks>
    /// <param name="id">Identificador GUID do administrador.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Resultado sem conteúdo quando removido.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var adminId)) return BadRequest();
        var entity = await _factory.AdminUsers.GetByIdAsync(adminId, ct);
        if (entity is null) return NotFound();
        await _factory.AdminUsers.DeleteAsync(entity, ct);
        return NoContent();
    }

    private static AdminUserResponse ToResponse(AdminUser entity)
    {
        return new AdminUserResponse
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            Email = entity.Email,
            Role = ToFriendlyRole(entity.Role),
            IsActive = entity.IsActive,
        };
    }

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
        _ => "Administrador",
    };

    private async Task EnsureSystemAdminsAsync(CancellationToken ct)
    {
        var defaults = new[]
        {
            new { Name = "Secretaria OAB Cidade Exemplo", Email = "admin@subsecao.pro", Role = "admin", Password = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD") ?? Guid.NewGuid().ToString("N") },
            new { Name = "Moderador Hub", Email = "moderador@subsecao.pro", Role = "moderator", Password = Environment.GetEnvironmentVariable("SEED_MODERATOR_PASSWORD") ?? Guid.NewGuid().ToString("N") },
        };

        var changed = false;

        foreach (var model in defaults)
        {
            var existing = await _factory.AdminUsers.GetByEmailAsync(model.Email, ct);
            if (existing is null)
            {
                await _factory.AdminUsers.AddAsync(new AdminUser
                {
                    Id = Guid.NewGuid(),
                    SubsectionId = _tenantContext.RequireSubsectionId(),
                    Name = model.Name,
                    Email = model.Email,
                    Role = model.Role,
                    IsActive = true,
                    PasswordHash = _passwordHasher.Hash(model.Password),
                }, ct);
                changed = true;
                continue;
            }

            if (!existing.IsActive || !string.Equals(existing.Role, model.Role, StringComparison.OrdinalIgnoreCase))
            {
                existing.IsActive = true;
                existing.Role = model.Role;
                await _factory.AdminUsers.UpdateAsync(existing, ct);
                changed = true;
            }
        }

        if (changed)
        {
            // no-op: Update/Add already persisted via repository, this branch exists
            // only to make intent explicit and keep future customizations predictable.
        }
    }
}

public class UpsertAdminUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Financeiro";
    public bool? IsActive { get; set; }
}

public class AdminUserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
