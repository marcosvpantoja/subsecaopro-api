using System.Text.Json;
using SubseOpro.Application.ViewModels.Platform;
using SubseOpro.Domain;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Application.Mappers;

public static class PlatformAppSettingsMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public static PublicModulesConfigViewModel ParseModules(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return CreateDefaultModules();
        try
        {
            return JsonSerializer.Deserialize<PublicModulesConfigViewModel>(json, JsonOptions)
                   ?? CreateDefaultModules();
        }
        catch
        {
            return CreateDefaultModules();
        }
    }

    public static string SerializeModules(PublicModulesConfigViewModel modules) =>
        JsonSerializer.Serialize(modules, JsonOptions);

    public static PlatformAppSettingsViewModel ToViewModel(
        PlatformAppSettings entity,
        IEnumerable<Subsection> activeSubsections)
    {
        return new PlatformAppSettingsViewModel
        {
            PublicAppEnabled = entity.PublicAppEnabled,
            PlatformName = entity.PlatformName,
            DefaultSubsectionSlug = entity.DefaultSubsectionSlug,
            AiMode = entity.AiMode,
            AiGlobalPrompt = entity.AiGlobalPrompt,
            AiDisclaimer = entity.AiDisclaimer,
            PublicModules = ParseModules(entity.PublicModulesJson),
            Subsections = activeSubsections
                .OrderBy(s => s.Name)
                .Select(s => new HubSubsectionRefViewModel
                {
                    Slug = s.Slug,
                    Name = s.Name,
                    PrimaryColor = s.PrimaryColor,
                    AccentColor = s.AccentColor,
                    LogoUrl = s.LogoUrl,
                })
                .ToList(),
        };
    }

    public static void ApplyUpdate(PlatformAppSettings entity, UpdatePlatformAppSettingsViewModel request)
    {
        if (request.PublicAppEnabled.HasValue) entity.PublicAppEnabled = request.PublicAppEnabled.Value;
        if (request.PlatformName is not null) entity.PlatformName = request.PlatformName.Trim();
        if (request.DefaultSubsectionSlug is not null)
            entity.DefaultSubsectionSlug = string.IsNullOrWhiteSpace(request.DefaultSubsectionSlug)
                ? null
                : request.DefaultSubsectionSlug.Trim().ToLowerInvariant();
        if (request.AiMode is not null) entity.AiMode = request.AiMode.Trim().ToLowerInvariant();
        if (request.AiGlobalPrompt is not null)
            entity.AiGlobalPrompt = string.IsNullOrWhiteSpace(request.AiGlobalPrompt) ? null : request.AiGlobalPrompt.Trim();
        if (request.AiDisclaimer is not null)
            entity.AiDisclaimer = string.IsNullOrWhiteSpace(request.AiDisclaimer) ? null : request.AiDisclaimer.Trim();
        if (request.PublicModules is not null)
            entity.PublicModulesJson = SerializeModules(NormalizeModules(request.PublicModules));
    }

    public static PlatformAppSettings CreateDefault() =>
        new()
        {
            Id = PlatformDefaults.AppSettingsId,
            PublicAppEnabled = false,
            PlatformName = "SubseçãoPRO",
            AiMode = "mock",
            PublicModulesJson = SerializeModules(CreateDefaultModules()),
        };

    public static PublicModulesConfigViewModel CreateDefaultModules() =>
        new()
        {
            Lawyers = new HubModuleConfigViewModel { Enabled = true },
            Banners = new HubModuleConfigViewModel { Enabled = true },
            Posts = new HubModuleConfigViewModel { Enabled = true },
            Articles = new HubModuleConfigViewModel { Enabled = true },
            About = new HubModuleConfigViewModel { Enabled = true },
            Elections = new HubModuleConfigViewModel { Enabled = true },
            Notifications = new HubModuleConfigViewModel(),
        };

    private static PublicModulesConfigViewModel NormalizeModules(PublicModulesConfigViewModel modules)
    {
        modules.Lawyers.SubsectionSlugs = NormalizeSlugs(modules.Lawyers.SubsectionSlugs);
        modules.Banners.SubsectionSlugs = NormalizeSlugs(modules.Banners.SubsectionSlugs);
        modules.Posts.SubsectionSlugs = NormalizeSlugs(modules.Posts.SubsectionSlugs);
        modules.Articles.SubsectionSlugs = NormalizeSlugs(modules.Articles.SubsectionSlugs);
        modules.About.SubsectionSlugs = NormalizeSlugs(modules.About.SubsectionSlugs);
        modules.Elections.SubsectionSlugs = NormalizeSlugs(modules.Elections.SubsectionSlugs);
        modules.Notifications.SubsectionSlugs = NormalizeSlugs(modules.Notifications.SubsectionSlugs);
        return modules;
    }

    private static List<string> NormalizeSlugs(IEnumerable<string> slugs) =>
        slugs
            .Select(s => s.Trim().ToLowerInvariant())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();

    public static List<string> ResolveModuleSlugs(
        HubModuleConfigViewModel module,
        IReadOnlyList<Subsection> activeSubsections,
        IReadOnlyList<string>? querySlugs = null)
    {
        if (!module.Enabled) return [];

        var activeSlugs = activeSubsections.Select(s => s.Slug).ToList();
        return ResolveModuleSlugs(module, activeSlugs, querySlugs);
    }

    public static List<string> ResolveModuleSlugs(
        HubModuleConfigViewModel module,
        IReadOnlyList<string> activeSlugs,
        IReadOnlyList<string>? querySlugs = null)
    {
        if (!module.Enabled) return [];

        var configured = module.SubsectionSlugs.Count > 0
            ? module.SubsectionSlugs
            : activeSlugs.ToList();

        if (querySlugs is { Count: > 0 })
            configured = configured.Intersect(querySlugs, StringComparer.OrdinalIgnoreCase).ToList();

        var activeSet = activeSlugs.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return configured.Where(activeSet.Contains).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }
}
