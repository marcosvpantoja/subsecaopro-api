using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubseOpro.Application.Mappers;
using SubseOpro.Application.Services;
using SubseOpro.Application.ViewModels.Subsection;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Interfaces;
using SubseOpro.Infrastructure.Persistence;

namespace SubseOpro.Api.Controllers;

[ApiController]
[Route("api/v1/platform/subsections")]
[Authorize(Policy = "PlatformAdmin")]
public class PlatformSubsectionsController : ControllerBase
{
    private readonly IRepositoryFactory _factory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAdminWelcomeEmailService _welcomeEmailService;
    private readonly AppDbContext _db;

    public PlatformSubsectionsController(
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
    public async Task<ActionResult<IReadOnlyList<SubsectionListItemViewModel>>> List(CancellationToken ct)
    {
        var items = await _factory.Subsections.GetAllAsync(ct);
        var adminCounts = await _db.AdminUsers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .GroupBy(a => a.SubsectionId)
            .Select(g => new { SubsectionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SubsectionId, x => x.Count, ct);

        return Ok(items.Select(s =>
            SubsectionMapper.ToListItem(s, adminCounts.GetValueOrDefault(s.Id, 0))).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubsectionInfoViewModel>> Get(Guid id, CancellationToken ct)
    {
        var entity = await _factory.Subsections.GetByIdAsync(id, ct);
        if (entity is null) return NotFound();

        var adminCount = await _db.AdminUsers
            .IgnoreQueryFilters()
            .CountAsync(a => a.SubsectionId == id, ct);

        return Ok(SubsectionMapper.ToInfoViewModel(entity, adminCount: adminCount));
    }

    [HttpGet("{id:guid}/deploy-checklist")]
    public async Task<ActionResult<SubsectionDeployChecklistViewModel>> DeployChecklist(Guid id, CancellationToken ct)
    {
        var entity = await _factory.Subsections.GetByIdAsync(id, ct);
        if (entity is null) return NotFound();

        var host = entity.DedicatedHubHost ?? $"{entity.Slug}.subse-pro.omnora.tech";
        var steps = new List<string>();

        if (entity.HubAccessMode == Domain.Enums.HubAccessMode.DedicatedSubdomain)
        {
            steps.Add($"Cloudflare: CNAME {entity.Slug}.subse-pro → hub (wildcard Caddy já cobre *.subse-pro.omnora.tech)");
            steps.Add($"Acessar backoffice em https://{host}/login");
        }
        else
        {
            steps.Add($"Hub compartilhado (padrão): login em /login — identificador \"{entity.Slug}\" no JWT");
            steps.Add("Local Docker: http://localhost:5173/login (sem subdomínio)");
            steps.Add("Produção: https://subse-pro.omnora.tech/login");
        }

        if (entity.MobileAccessMode == Domain.Enums.MobileAccessMode.WhiteLabelBuild)
        {
            var profile = entity.EasProfileName ?? $"production-{entity.Slug}";
            steps.Add($"EAS: eas build --profile {profile}");
            steps.Add($"Definir EXPO_PUBLIC_TENANT_SLUG={entity.Slug} no profile {profile}");
        }
        else
        {
            steps.Add("App único na loja: tenant resolvido automaticamente após login");
        }

        var resolve = await _factory.Subsections.GetBySlugAsync(entity.Slug, ct);
        return Ok(new SubsectionDeployChecklistViewModel
        {
            Slug = entity.Slug,
            HubAccessMode = entity.HubAccessMode.ToString(),
            MobileAccessMode = entity.MobileAccessMode.ToString(),
            DedicatedHubHost = host,
            EasProfileName = entity.EasProfileName,
            SlugResolveOk = resolve is not null && resolve.IsActive,
            SlugResolveMessage = resolve is null
                ? "Identificador (slug) não encontrado na API"
                : resolve.IsActive
                    ? $"Identificador \"{entity.Slug}\" ativo — subseção resolvida no login"
                    : "Subseção inativa",
            DeploySteps = steps,
        });
    }

    [HttpPost("{id:guid}/bootstrap")]
    public async Task<ActionResult<SubsectionBootstrapResultViewModel>> Bootstrap(Guid id, CancellationToken ct)
    {
        var entity = await _factory.Subsections.GetByIdAsync(id, ct);
        if (entity is null) return NotFound();

        var result = new SubsectionBootstrapResultViewModel();

        if (!await _db.Banners.IgnoreQueryFilters().AnyAsync(b => b.SubsectionId == id, ct))
        {
            await _db.Banners.AddAsync(new Banner
            {
                Id = Guid.NewGuid(),
                SubsectionId = id,
                Title = $"Bem-vindo à {entity.Name}",
                Subtitle = "Plataforma digital da subseção",
                Cta = "Saiba mais",
                LinkTo = "/about",
                LinkParams = new Dictionary<string, string>(),
                PhotoUrl = "",
                Color = entity.PrimaryColor,
                SortOrder = 0,
            }, ct);
            result.BannersCreated = 1;
        }

        if (!await _db.Rooms.IgnoreQueryFilters().AnyAsync(r => r.SubsectionId == id, ct))
        {
            await _db.Rooms.AddAsync(new Room
            {
                Id = Guid.NewGuid(),
                SubsectionId = id,
                Name = "Sala de Reunião",
                Description = "Ambiente para reuniões",
                Capacity = 6,
                Features = ["Wi-Fi", "Ar-condicionado"],
                PricePerHour = 0,
                IsFree = true,
                Status = Domain.Enums.RoomStatus.Available,
                Color = "from-blue-500 to-indigo-600",
            }, ct);
            result.RoomsCreated = 1;
        }

        if (!await _db.SocialAccounts.IgnoreQueryFilters().AnyAsync(s => s.SubsectionId == id, ct))
        {
            await _db.SocialAccounts.AddRangeAsync([
                new SocialAccount
                {
                    Id = Guid.NewGuid(),
                    SubsectionId = id,
                    Platform = "instagram",
                    Handle = $"@{entity.Slug}",
                    ProfileUrl = $"https://instagram.com/{entity.Slug}",
                    Connected = false,
                },
                new SocialAccount
                {
                    Id = Guid.NewGuid(),
                    SubsectionId = id,
                    Platform = "facebook",
                    Handle = entity.Name,
                    ProfileUrl = $"https://facebook.com/{entity.Slug}",
                    Connected = false,
                },
            ], ct);
            result.SocialAccountsCreated = 2;
        }

        result.Created = result.BannersCreated + result.RoomsCreated + result.SocialAccountsCreated > 0;
        await _db.SaveChangesAsync(ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<SubsectionInfoViewModel>> Create(
        [FromBody] CreateSubsectionViewModel request, CancellationToken ct)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(slug) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { detail = "Nome e slug são obrigatórios." });

        var existing = await _factory.Subsections.GetBySlugAsync(slug, ct);
        if (existing is not null)
            return Conflict(new { detail = "Slug já está em uso." });

        var subsection = new Subsection
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = slug,
            IsActive = true,
            LogoUrl = request.LogoUrl,
            PrimaryColor = request.PrimaryColor,
            AccentColor = request.AccentColor,
            Description = request.Description,
            Address = request.Address,
            Phone = request.Phone,
            Email = request.Email,
            Hours = request.Hours,
            CreatedAt = DateTime.UtcNow,
            IaEnabled = request.IaEnabled ?? true,
            IaDisclaimer = request.IaDisclaimer ?? SubsectionIaDefaults.DefaultDisclaimer,
            IaMaxTurns = request.IaMaxTurns ?? SubsectionIaDefaults.DefaultMaxTurns,
            IaAreasCobertasJson = SubsectionMapper.SerializeAreas(request.IaAreasCobertas),
            IaSystemPrompt = string.IsNullOrWhiteSpace(request.IaSystemPrompt) ? null : request.IaSystemPrompt.Trim(),
        };

        SubsectionMapper.ApplyAccessModes(
            subsection,
            request.HubAccessMode,
            request.MobileAccessMode,
            request.DedicatedHubHost,
            request.EasProfileName);

        await _factory.Subsections.AddAsync(subsection, ct);

        AdminUser? createdAdmin = null;
        string? adminPlainPassword = null;

        if (!string.IsNullOrWhiteSpace(request.AdminEmail) &&
            !string.IsNullOrWhiteSpace(request.AdminPassword) &&
            !string.IsNullOrWhiteSpace(request.AdminName))
        {
            if (!PasswordPolicy.IsValid(request.AdminPassword, out var policyError))
                return BadRequest(new { detail = policyError });

            if (await _factory.AdminUsers.EmailExistsGloballyAsync(request.AdminEmail.Trim(), ct: ct))
                return Conflict(new { detail = "E-mail do administrador já está em uso." });

            createdAdmin = new AdminUser
            {
                Id = Guid.NewGuid(),
                SubsectionId = subsection.Id,
                Name = request.AdminName.Trim(),
                Email = request.AdminEmail.Trim(),
                PasswordHash = _passwordHasher.Hash(request.AdminPassword),
                Role = "admin",
                IsActive = true,
            };
            adminPlainPassword = request.AdminPassword;

            await _db.AdminUsers.AddAsync(createdAdmin, ct);
        }

        await _factory.Subsections.SaveChangesAsync(ct);

        var welcomeEmailSent = false;
        if (createdAdmin is not null && adminPlainPassword is not null)
        {
            welcomeEmailSent = await _welcomeEmailService.TrySendWelcomeAsync(
                createdAdmin, subsection, adminPlainPassword, ct);
        }

        var adminCount = createdAdmin is not null ? 1 : 0;
        var response = SubsectionMapper.ToInfoViewModel(subsection, adminCount: adminCount);
        response.WelcomeEmailSent = createdAdmin is not null ? welcomeEmailSent : null;

        return CreatedAtAction(nameof(Get), new { id = subsection.Id }, response);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<SubsectionInfoViewModel>> Update(
        Guid id, [FromBody] UpdateSubsectionViewModel request, CancellationToken ct)
    {
        var entity = await _factory.Subsections.GetByIdAsync(id, ct);
        if (entity is null) return NotFound();

        if (request.Name is not null) entity.Name = request.Name.Trim();
        if (request.Slug is not null)
        {
            var slug = request.Slug.Trim().ToLowerInvariant();
            var existing = await _factory.Subsections.GetBySlugAsync(slug, ct);
            if (existing is not null && existing.Id != id)
                return Conflict(new { detail = "Slug já está em uso." });
            entity.Slug = slug;
        }
        if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;
        if (request.LogoUrl is not null) entity.LogoUrl = request.LogoUrl;
        if (request.PrimaryColor is not null) entity.PrimaryColor = request.PrimaryColor;
        if (request.AccentColor is not null) entity.AccentColor = request.AccentColor;
        if (request.Description is not null) entity.Description = request.Description;
        if (request.Address is not null) entity.Address = request.Address;
        if (request.Phone is not null) entity.Phone = request.Phone;
        if (request.Email is not null) entity.Email = request.Email;
        if (request.Hours is not null) entity.Hours = request.Hours;
        SubsectionMapper.ApplyIaFields(
            entity,
            request.IaEnabled,
            request.IaSystemPrompt,
            request.IaDisclaimer,
            request.IaMaxTurns,
            request.IaAreasCobertas);
        SubsectionMapper.ApplyAccessModes(
            entity,
            request.HubAccessMode,
            request.MobileAccessMode,
            request.DedicatedHubHost,
            request.EasProfileName);
        if (request.AiServiceUrl is not null)
            entity.AiServiceUrl = string.IsNullOrWhiteSpace(request.AiServiceUrl) ? null : request.AiServiceUrl.Trim();

        await _factory.Subsections.UpdateAsync(entity, ct);
        await _factory.Subsections.SaveChangesAsync(ct);

        var adminCount = await _db.AdminUsers
            .IgnoreQueryFilters()
            .CountAsync(a => a.SubsectionId == id, ct);

        return Ok(SubsectionMapper.ToInfoViewModel(entity, adminCount: adminCount));
    }
}
