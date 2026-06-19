using SubseOpro.Application.ViewModels.Article;
using SubseOpro.Application.ViewModels.Banner;
using SubseOpro.Application.ViewModels.Election;
using SubseOpro.Application.ViewModels.Lawyer;
using SubseOpro.Application.ViewModels.Notification;
using SubseOpro.Application.ViewModels.Post;

namespace SubseOpro.Application.ViewModels.Hub;

public class SubsectionOriginFields
{
    public string SubsectionSlug { get; set; } = string.Empty;
    public string SubsectionName { get; set; } = string.Empty;
    public string? SubsectionPrimaryColor { get; set; }
    public string? SubsectionAccentColor { get; set; }
    public string? SubsectionLogoUrl { get; set; }
}

public class HubPostViewModel : PostViewModel
{
    public string SubsectionSlug { get; set; } = string.Empty;
    public string SubsectionName { get; set; } = string.Empty;
    public string? SubsectionPrimaryColor { get; set; }
    public string? SubsectionAccentColor { get; set; }
    public string? SubsectionLogoUrl { get; set; }
}

public class HubBannerViewModel : BannerViewModel
{
    public string SubsectionSlug { get; set; } = string.Empty;
    public string SubsectionName { get; set; } = string.Empty;
    public string? SubsectionPrimaryColor { get; set; }
    public string? SubsectionAccentColor { get; set; }
    public string? SubsectionLogoUrl { get; set; }
}

public class HubLawyerViewModel : LawyerViewModel
{
    public string SubsectionSlug { get; set; } = string.Empty;
    public string SubsectionName { get; set; } = string.Empty;
    public string? SubsectionPrimaryColor { get; set; }
    public string? SubsectionAccentColor { get; set; }
    public string? SubsectionLogoUrl { get; set; }
}

public class HubArticleViewModel : ArticleViewModel
{
    public string SubsectionSlug { get; set; } = string.Empty;
    public string SubsectionName { get; set; } = string.Empty;
    public string? SubsectionPrimaryColor { get; set; }
    public string? SubsectionAccentColor { get; set; }
    public string? SubsectionLogoUrl { get; set; }
}

public class HubNotificationViewModel : NotificationViewModel
{
    public string SubsectionSlug { get; set; } = string.Empty;
    public string SubsectionName { get; set; } = string.Empty;
    public string? SubsectionPrimaryColor { get; set; }
    public string? SubsectionAccentColor { get; set; }
    public string? SubsectionLogoUrl { get; set; }
}

public class HubElectionViewModel : ElectionViewModel
{
    public string SubsectionSlug { get; set; } = string.Empty;
    public string SubsectionName { get; set; } = string.Empty;
    public string? SubsectionPrimaryColor { get; set; }
    public string? SubsectionAccentColor { get; set; }
    public string? SubsectionLogoUrl { get; set; }
}
