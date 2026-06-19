using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubseOpro.Api.Services;
using SubseOpro.Application.ViewModels.PersonalAccessToken;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Interfaces;

namespace SubseOpro.Api.Controllers;

[ApiController]
[Route("api/v1/platform/api-tokens")]
[Authorize(Policy = "PlatformAdmin")]
public class PlatformPersonalAccessTokensController : ControllerBase
{
    private readonly IRepositoryFactory _factory;
    private readonly AiTokenEncryptionService _encryption;

    public PlatformPersonalAccessTokensController(
        IRepositoryFactory factory,
        AiTokenEncryptionService encryption)
    {
        _factory = factory;
        _encryption = encryption;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PatViewModel>>> List(
        [FromQuery] Guid subsectionId, CancellationToken ct)
    {
        if (subsectionId == Guid.Empty)
            return BadRequest(new { detail = "subsectionId é obrigatório." });

        var subsection = await _factory.Subsections.GetByIdAsync(subsectionId, ct);
        if (subsection is null)
            return NotFound(new { detail = "Subseção não encontrada." });

        var tokens = await _factory.PersonalAccessTokens.GetBySubsectionIdAsync(subsectionId, ct);

        return Ok(tokens.Select(p => MapToViewModel(p, subsection.Slug, subsection.ActiveAiTokenId)).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<PatCreatedViewModel>> Create(
        [FromBody] CreatePatRequestViewModel request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { detail = "Nome é obrigatório." });

        if (request.SubsectionId == Guid.Empty)
            return BadRequest(new { detail = "SubsectionId é obrigatório." });

        if (request.ExpiresAt.HasValue && request.ExpiresAt.Value <= DateTimeOffset.UtcNow)
            return BadRequest(new { detail = "ExpiresAt deve ser uma data futura." });

        var subsection = await _factory.Subsections.GetByIdAsync(request.SubsectionId, ct);
        if (subsection is null)
            return NotFound(new { detail = "Subseção não encontrada." });

        if (!subsection.IsActive)
            return BadRequest(new { detail = "Subseção inativa." });

        var rawBytes = RandomNumberGenerator.GetBytes(32);
        var rawToken = "spro_" + Convert.ToHexString(rawBytes).ToLowerInvariant();
        var tokenHash = TokenHasher.ComputeSha256Hex(rawToken);

        var pat = new PersonalAccessToken
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            TokenHash = tokenHash,
            SubsectionId = request.SubsectionId,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = request.ExpiresAt,
            IsRevoked = false,
        };

        await _factory.PersonalAccessTokens.AddAsync(pat, ct);

        string? encryptedToken = null;

        if (request.SetAsActiveAiToken)
        {
            encryptedToken = _encryption.Encrypt(rawToken);
            subsection.ActiveAiTokenEncrypted = encryptedToken;
            subsection.ActiveAiTokenId = pat.Id;
            await _factory.Subsections.UpdateAsync(subsection, ct);
            await _factory.Subsections.SaveChangesAsync(ct);
        }

        var vm = MapToViewModel(pat, subsection.Slug, subsection.ActiveAiTokenId);

        return CreatedAtAction(nameof(List), new { subsectionId = pat.SubsectionId },
            new PatCreatedViewModel
            {
                Pat = vm,
                Token = rawToken,
                EncryptedToken = encryptedToken,
            });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken ct)
    {
        var pat = await _factory.PersonalAccessTokens.GetByIdAsync(id, ct);
        if (pat is null)
            return NotFound(new { detail = "Token não encontrado." });

        if (pat.IsRevoked)
            return BadRequest(new { detail = "Token já foi revogado." });

        await _factory.PersonalAccessTokens.RevokeAsync(pat, ct);

        return NoContent();
    }

    private static PatViewModel MapToViewModel(
        PersonalAccessToken p,
        string subsectionSlug,
        Guid? activeAiTokenId) => new()
    {
        Id = p.Id,
        Name = p.Name,
        SubsectionId = p.SubsectionId,
        SubsectionSlug = subsectionSlug,
        CreatedAt = p.CreatedAt,
        ExpiresAt = p.ExpiresAt,
        LastUsedAt = p.LastUsedAt,
        IsRevoked = p.IsRevoked,
        IsExpired = p.ExpiresAt.HasValue && p.ExpiresAt.Value <= DateTimeOffset.UtcNow,
        IsActiveAiToken = activeAiTokenId.HasValue && activeAiTokenId.Value == p.Id,
    };
}
