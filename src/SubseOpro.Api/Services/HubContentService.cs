using Microsoft.EntityFrameworkCore;
using SubseOpro.Application.Mappers;
using SubseOpro.Application.ViewModels.Hub;
using SubseOpro.Application.ViewModels.Platform;
using SubseOpro.Domain.Entities;
using SubseOpro.Infrastructure.Persistence;

namespace SubseOpro.Api.Services;

public class HubContentService
{
    private readonly AppDbContext _db;

    public HubContentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PlatformAppSettings> GetOrCreateSettingsAsync(CancellationToken ct)
    {
        var entity = await _db.PlatformAppSettings
            .FirstOrDefaultAsync(x => x.Id == Domain.PlatformDefaults.AppSettingsId, ct);

        if (entity is not null) return entity;

        entity = PlatformAppSettingsMapper.CreateDefault();
        await _db.PlatformAppSettings.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<List<Subsection>> GetActiveSubsectionsAsync(CancellationToken ct) =>
        await _db.Subsections
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

    public async Task<PlatformAppSettingsViewModel> BuildConfigViewModelAsync(CancellationToken ct)
    {
        var settings = await GetOrCreateSettingsAsync(ct);
        var subsections = await GetActiveSubsectionsAsync(ct);
        return PlatformAppSettingsMapper.ToViewModel(settings, subsections);
    }

    public async Task SaveSettingsAsync(Domain.Entities.PlatformAppSettings entity, CancellationToken ct)
    {
        _db.PlatformAppSettings.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public List<string> ResolveSlugs(
        HubModuleConfigViewModel module,
        IReadOnlyList<Subsection> subsections,
        IReadOnlyList<string>? querySlugs) =>
        PlatformAppSettingsMapper.ResolveModuleSlugs(module, subsections, querySlugs);

    private Dictionary<string, Subsection> SubsectionMap(IReadOnlyList<Subsection> subsections) =>
        subsections.ToDictionary(s => s.Slug, StringComparer.OrdinalIgnoreCase);

    public async Task<List<HubPostViewModel>> GetPostsAsync(
        IReadOnlyList<string> slugs,
        CancellationToken ct)
    {
        if (slugs.Count == 0) return [];

        var subsectionIds = await SubsectionIdsAsync(slugs, ct);
        if (subsectionIds.Count == 0) return [];

        var subsections = await GetActiveSubsectionsAsync(ct);
        var map = SubsectionMap(subsections);

        var posts = await _db.Posts
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(p => subsectionIds.Contains(p.SubsectionId) && p.IsActive)
            .OrderByDescending(p => p.PublishedAt)
            .Take(100)
            .ToListAsync(ct);

        return posts
            .Select(p =>
            {
                var sub = subsections.First(s => s.Id == p.SubsectionId);
                return HubContentMapper.ToHubPost(p, sub);
            })
            .ToList();
    }

    public async Task<List<HubBannerViewModel>> GetBannersAsync(
        IReadOnlyList<string> slugs,
        CancellationToken ct)
    {
        if (slugs.Count == 0) return [];

        var subsectionIds = await SubsectionIdsAsync(slugs, ct);
        if (subsectionIds.Count == 0) return [];

        var subsections = await GetActiveSubsectionsAsync(ct);

        var banners = await _db.Banners
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(b => subsectionIds.Contains(b.SubsectionId))
            .OrderBy(b => b.SortOrder)
            .ToListAsync(ct);

        return banners
            .Select(b =>
            {
                var sub = subsections.First(s => s.Id == b.SubsectionId);
                return HubContentMapper.ToHubBanner(b, sub);
            })
            .ToList();
    }

    public async Task<List<HubLawyerViewModel>> GetLawyersAsync(
        IReadOnlyList<string> slugs,
        string? q,
        string? specialty,
        string? city,
        bool? online,
        CancellationToken ct)
    {
        if (slugs.Count == 0) return [];

        var subsectionIds = await SubsectionIdsAsync(slugs, ct);
        if (subsectionIds.Count == 0) return [];

        var subsections = await GetActiveSubsectionsAsync(ct);

        var query = _db.Lawyers
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(l => l.LawyerSpecialties)
            .ThenInclude(ls => ls.Specialty)
            .Where(l => subsectionIds.Contains(l.SubsectionId) && l.IsActive);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLowerInvariant();
            query = query.Where(l =>
                l.Name.ToLower().Contains(term) ||
                l.OabNumber.Contains(term) ||
                l.City.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(l => l.City.ToLower().Contains(city.Trim().ToLowerInvariant()));

        if (online == true)
            query = query.Where(l => l.Online);

        if (!string.IsNullOrWhiteSpace(specialty))
        {
            var spec = specialty.Trim().ToLowerInvariant();
            query = query.Where(l =>
                l.LawyerSpecialties.Any(ls => ls.Specialty.Name.ToLower().Contains(spec)));
        }

        var lawyers = await query.OrderBy(l => l.Name).Take(200).ToListAsync(ct);

        return lawyers
            .Select(l =>
            {
                var sub = subsections.First(s => s.Id == l.SubsectionId);
                return HubContentMapper.ToHubLawyer(l, sub);
            })
            .ToList();
    }

    public async Task<List<HubArticleViewModel>> GetArticlesAsync(
        IReadOnlyList<string> slugs,
        CancellationToken ct)
    {
        if (slugs.Count == 0) return [];

        var subsectionIds = await SubsectionIdsAsync(slugs, ct);
        if (subsectionIds.Count == 0) return [];

        var subsections = await GetActiveSubsectionsAsync(ct);

        var articles = await _db.Articles
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(a => subsectionIds.Contains(a.SubsectionId))
            .OrderBy(a => a.Title)
            .ToListAsync(ct);

        return articles
            .Select(a =>
            {
                var sub = subsections.First(s => s.Id == a.SubsectionId);
                return HubContentMapper.ToHubArticle(a, sub);
            })
            .ToList();
    }

    public async Task<List<HubNotificationViewModel>> GetNotificationsAsync(CancellationToken ct)
    {
        var subsections = await GetActiveSubsectionsAsync(ct);
        if (subsections.Count == 0) return [];

        var subsectionIds = subsections.Select(s => s.Id).ToList();

        var notifications = await _db.Notifications
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(n => subsectionIds.Contains(n.SubsectionId))
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync(ct);

        return notifications
            .Select(n =>
            {
                var sub = subsections.First(s => s.Id == n.SubsectionId);
                return HubContentMapper.ToHubNotification(n, sub);
            })
            .ToList();
    }

    public async Task<List<HubElectionViewModel>> GetActiveElectionsAsync(
        IReadOnlyList<string> slugs,
        CancellationToken ct)
    {
        if (slugs.Count == 0) return [];

        var subsectionIds = await SubsectionIdsAsync(slugs, ct);
        if (subsectionIds.Count == 0) return [];

        var subsections = await GetActiveSubsectionsAsync(ct);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var elections = await _db.Elections
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(e => e.Candidates)
            .Where(e => subsectionIds.Contains(e.SubsectionId) && e.EndDate >= today)
            .OrderBy(e => e.EndDate)
            .ToListAsync(ct);

        return elections
            .Select(e =>
            {
                var sub = subsections.First(s => s.Id == e.SubsectionId);
                return HubContentMapper.ToHubElection(e, sub);
            })
            .ToList();
    }

    private async Task<List<Guid>> SubsectionIdsAsync(IReadOnlyList<string> slugs, CancellationToken ct)
    {
        var normalized = slugs.Select(s => s.Trim().ToLowerInvariant()).ToList();
        return await _db.Subsections
            .AsNoTracking()
            .Where(s => s.IsActive && normalized.Contains(s.Slug))
            .Select(s => s.Id)
            .ToListAsync(ct);
    }
}
