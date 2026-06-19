using System.Text.Json;
using SubseOpro.Application.Services;
using SubseOpro.Application.ViewModels.Subsection;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Enums;

namespace SubseOpro.Application.Mappers;

public static class SubsectionMapper
{
    public static SubsectionInfoViewModel ToInfoViewModel(
        Subsection entity,
        IEnumerable<SocialAccount>? socialAccounts = null,
        int adminCount = 0) =>
        new()
        {
            Name = entity.Name,
            Slug = entity.Slug,
            IsActive = entity.IsActive,
            LogoUrl = entity.LogoUrl,
            PrimaryColor = entity.PrimaryColor,
            AccentColor = entity.AccentColor,
            Description = entity.Description,
            Address = entity.Address,
            Phone = entity.Phone,
            Email = entity.Email,
            Hours = entity.Hours,
            IaEnabled = entity.IaEnabled,
            IaSystemPrompt = string.IsNullOrWhiteSpace(entity.IaSystemPrompt)
                ? SubsectionIaDefaults.BuildSystemPrompt(entity.Name)
                : entity.IaSystemPrompt,
            IaDisclaimer = string.IsNullOrWhiteSpace(entity.IaDisclaimer)
                ? SubsectionIaDefaults.DefaultDisclaimer
                : entity.IaDisclaimer,
            IaMaxTurns = entity.IaMaxTurns > 0 ? entity.IaMaxTurns : SubsectionIaDefaults.DefaultMaxTurns,
            IaAreasCobertas = ParseAreas(entity.IaAreasCobertasJson),
            SocialLinks = (socialAccounts ?? [])
                .Where(s => s.Connected)
                .Select(s => new SubsectionSocialLinkViewModel
                {
                    Platform = s.Platform,
                    Handle = s.Handle,
                    ProfileUrl = s.ProfileUrl,
                })
                .ToList(),
            HubAccessMode = entity.HubAccessMode.ToString(),
            MobileAccessMode = entity.MobileAccessMode.ToString(),
            DedicatedHubHost = entity.DedicatedHubHost,
            EasProfileName = entity.EasProfileName,
            AiServiceUrl = entity.AiServiceUrl,
            AdminCount = adminCount,
        };

    public static SubsectionResolveViewModel ToResolveViewModel(Subsection entity) =>
        new()
        {
            Name = entity.Name,
            Slug = entity.Slug,
            IsActive = entity.IsActive,
            LogoUrl = entity.LogoUrl,
            PrimaryColor = entity.PrimaryColor,
            AccentColor = entity.AccentColor,
            ActiveAiTokenEncrypted = entity.ActiveAiTokenEncrypted,
        };

    public static SubsectionListItemViewModel ToListItem(Subsection entity, int adminCount = 0) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Slug = entity.Slug,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            AdminCount = adminCount,
            HubAccessMode = entity.HubAccessMode.ToString(),
            MobileAccessMode = entity.MobileAccessMode.ToString(),
            HasDedicatedSubdomain = entity.HubAccessMode == HubAccessMode.DedicatedSubdomain,
        };

    public static string SerializeAreas(IReadOnlyList<string>? areas)
    {
        var list = areas?.Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Trim()).Distinct().ToList()
            ?? [];
        if (list.Count == 0)
            list = SubsectionIaDefaults.DefaultAreas.ToList();
        return JsonSerializer.Serialize(list);
    }

    public static IReadOnlyList<string> ParseAreas(string? json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "[]")
            return SubsectionIaDefaults.DefaultAreas;

        try
        {
            var parsed = JsonSerializer.Deserialize<List<string>>(json);
            if (parsed is null || parsed.Count == 0)
                return SubsectionIaDefaults.DefaultAreas;
            return parsed;
        }
        catch
        {
            return SubsectionIaDefaults.DefaultAreas;
        }
    }

    public static void ApplySettings(Subsection entity, UpdateSubsectionSettingsViewModel request)
    {
        if (request.Description is not null) entity.Description = request.Description.Trim();
        if (request.Address is not null) entity.Address = request.Address.Trim();
        if (request.Phone is not null) entity.Phone = request.Phone.Trim();
        if (request.Email is not null) entity.Email = request.Email.Trim();
        if (request.Hours is not null) entity.Hours = request.Hours.Trim();
        ApplyIaFields(entity, request.IaEnabled, request.IaSystemPrompt, request.IaDisclaimer, request.IaMaxTurns, request.IaAreasCobertas);
    }

    public static void ApplyIaFields(
        Subsection entity,
        bool? iaEnabled,
        string? iaSystemPrompt,
        string? iaDisclaimer,
        int? iaMaxTurns,
        IReadOnlyList<string>? iaAreasCobertas)
    {
        if (iaEnabled.HasValue) entity.IaEnabled = iaEnabled.Value;
        if (iaSystemPrompt is not null) entity.IaSystemPrompt = string.IsNullOrWhiteSpace(iaSystemPrompt) ? null : iaSystemPrompt.Trim();
        if (iaDisclaimer is not null) entity.IaDisclaimer = iaDisclaimer.Trim();
        if (iaMaxTurns.HasValue) entity.IaMaxTurns = iaMaxTurns.Value;
        if (iaAreasCobertas is not null) entity.IaAreasCobertasJson = SerializeAreas(iaAreasCobertas);
    }

    public static void ApplyAccessModes(
        Subsection entity,
        string? hubAccessMode,
        string? mobileAccessMode,
        string? dedicatedHubHost,
        string? easProfileName)
    {
        if (hubAccessMode is not null &&
            Enum.TryParse<HubAccessMode>(hubAccessMode, true, out var hubMode))
        {
            entity.HubAccessMode = hubMode;
        }

        if (mobileAccessMode is not null &&
            Enum.TryParse<MobileAccessMode>(mobileAccessMode, true, out var mobileMode))
        {
            entity.MobileAccessMode = mobileMode;
        }

        if (dedicatedHubHost is not null)
            entity.DedicatedHubHost = string.IsNullOrWhiteSpace(dedicatedHubHost) ? null : dedicatedHubHost.Trim();

        if (easProfileName is not null)
            entity.EasProfileName = string.IsNullOrWhiteSpace(easProfileName) ? null : easProfileName.Trim();

        if (entity.HubAccessMode == HubAccessMode.DedicatedSubdomain &&
            string.IsNullOrWhiteSpace(entity.DedicatedHubHost))
        {
            entity.DedicatedHubHost = $"{entity.Slug}.subse-pro.omnora.tech";
        }

        if (entity.MobileAccessMode == MobileAccessMode.WhiteLabelBuild &&
            string.IsNullOrWhiteSpace(entity.EasProfileName))
        {
            entity.EasProfileName = $"production-{entity.Slug}";
        }
    }
}
