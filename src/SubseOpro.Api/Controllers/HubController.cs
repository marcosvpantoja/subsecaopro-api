using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubseOpro.Api.Services;
using SubseOpro.Application.Mappers;
using SubseOpro.Application.ViewModels.Hub;
using SubseOpro.Application.ViewModels.Platform;

namespace SubseOpro.Api.Controllers;

/// <summary>
/// Feed agregado do app compartilhado (modo público, multi-subseção).
/// </summary>
[ApiController]
[Route("api/v1/hub")]
[AllowAnonymous]
public class HubController : ControllerBase
{
    private readonly HubContentService _hub;

    public HubController(HubContentService hub)
    {
        _hub = hub;
    }

    [HttpGet("config")]
    public async Task<ActionResult<PlatformAppSettingsViewModel>> GetConfig(CancellationToken ct)
    {
        var config = await _hub.BuildConfigViewModelAsync(ct);
        if (!config.PublicAppEnabled)
            return NotFound(new { detail = "App compartilhado não está habilitado." });
        return Ok(config);
    }

    [HttpGet("banners")]
    public async Task<ActionResult<List<HubBannerViewModel>>> GetBanners(
        [FromQuery] List<string> subsections,
        CancellationToken ct)
    {
        var config = await RequireEnabledAsync(ct);
        if (config is null) return NotFound();

        var slugs = ResolveSlugs(config, config.PublicModules.Banners, subsections);
        return Ok(await _hub.GetBannersAsync(slugs, ct));
    }

    [HttpGet("posts")]
    public async Task<ActionResult<List<HubPostViewModel>>> GetPosts(
        [FromQuery] List<string> subsections,
        CancellationToken ct)
    {
        var config = await RequireEnabledAsync(ct);
        if (config is null) return NotFound();

        var slugs = ResolveSlugs(config, config.PublicModules.Posts, subsections);
        return Ok(await _hub.GetPostsAsync(slugs, ct));
    }

    [HttpGet("lawyers")]
    public async Task<ActionResult<List<HubLawyerViewModel>>> GetLawyers(
        [FromQuery] List<string> subsections,
        [FromQuery] string? q,
        [FromQuery] string? specialty,
        [FromQuery] string? city,
        [FromQuery] bool? online,
        CancellationToken ct)
    {
        var config = await RequireEnabledAsync(ct);
        if (config is null) return NotFound();

        var slugs = ResolveSlugs(config, config.PublicModules.Lawyers, subsections);
        return Ok(await _hub.GetLawyersAsync(slugs, q, specialty, city, online, ct));
    }

    [HttpGet("articles")]
    public async Task<ActionResult<List<HubArticleViewModel>>> GetArticles(
        [FromQuery] List<string> subsections,
        CancellationToken ct)
    {
        var config = await RequireEnabledAsync(ct);
        if (config is null) return NotFound();

        var slugs = ResolveSlugs(config, config.PublicModules.Articles, subsections);
        return Ok(await _hub.GetArticlesAsync(slugs, ct));
    }

    [HttpGet("notifications")]
    public async Task<ActionResult<List<HubNotificationViewModel>>> GetNotifications(CancellationToken ct)
    {
        var config = await RequireEnabledAsync(ct);
        if (config is null) return NotFound();
        if (!config.PublicModules.Notifications.Enabled) return Ok(new List<HubNotificationViewModel>());

        return Ok(await _hub.GetNotificationsAsync(ct));
    }

    [HttpGet("elections/active")]
    public async Task<ActionResult<List<HubElectionViewModel>>> GetActiveElections(
        [FromQuery] List<string> subsections,
        CancellationToken ct)
    {
        var config = await RequireEnabledAsync(ct);
        if (config is null) return NotFound();

        var slugs = ResolveSlugs(config, config.PublicModules.Elections, subsections);
        return Ok(await _hub.GetActiveElectionsAsync(slugs, ct));
    }

    private async Task<PlatformAppSettingsViewModel?> RequireEnabledAsync(CancellationToken ct)
    {
        var config = await _hub.BuildConfigViewModelAsync(ct);
        return config.PublicAppEnabled ? config : null;
    }

    private static List<string> ResolveSlugs(
        PlatformAppSettingsViewModel config,
        HubModuleConfigViewModel module,
        List<string> querySlugs)
    {
        var activeSlugs = config.Subsections.Select(s => s.Slug).ToList();
        return PlatformAppSettingsMapper.ResolveModuleSlugs(
            module,
            activeSlugs,
            querySlugs.Count > 0 ? querySlugs : null);
    }
}
