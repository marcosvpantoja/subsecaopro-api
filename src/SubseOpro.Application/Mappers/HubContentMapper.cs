using SubseOpro.Application.Mappers;
using SubseOpro.Application.ViewModels.Article;
using SubseOpro.Application.ViewModels.Banner;
using SubseOpro.Application.ViewModels.Election;
using SubseOpro.Application.ViewModels.Hub;
using SubseOpro.Application.ViewModels.Lawyer;
using SubseOpro.Application.ViewModels.Notification;
using SubseOpro.Application.ViewModels.Post;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Application.Mappers;

public static class HubContentMapper
{
    public static HubPostViewModel ToHubPost(Post entity, Subsection subsection)
    {
        var baseVm = PostMapper.ToViewModel(entity);
        return new HubPostViewModel
        {
            Id = baseVm.Id,
            Title = baseVm.Title,
            Date = baseVm.Date,
            Image = baseVm.Image,
            Photo = baseVm.Photo,
            Caption = baseVm.Caption,
            Link = baseVm.Link,
            Source = baseVm.Source,
            IsActive = baseVm.IsActive,
            Likes = baseVm.Likes,
            SubsectionSlug = subsection.Slug,
            SubsectionName = subsection.Name,
            SubsectionPrimaryColor = subsection.PrimaryColor,
            SubsectionAccentColor = subsection.AccentColor,
            SubsectionLogoUrl = subsection.LogoUrl,
        };
    }

    public static HubBannerViewModel ToHubBanner(Banner entity, Subsection subsection)
    {
        var baseVm = BannerMapper.ToViewModel(entity);
        return new HubBannerViewModel
        {
            Id = baseVm.Id,
            Title = baseVm.Title,
            Subtitle = baseVm.Subtitle,
            Cta = baseVm.Cta,
            Color = baseVm.Color,
            Photo = baseVm.Photo,
            To = baseVm.To,
            Params = baseVm.Params,
            SubsectionSlug = subsection.Slug,
            SubsectionName = subsection.Name,
            SubsectionPrimaryColor = subsection.PrimaryColor,
            SubsectionAccentColor = subsection.AccentColor,
            SubsectionLogoUrl = subsection.LogoUrl,
        };
    }

    public static HubLawyerViewModel ToHubLawyer(Lawyer entity, Subsection subsection)
    {
        var baseVm = LawyerMapper.ToViewModel(entity);
        return new HubLawyerViewModel
        {
            Id = baseVm.Id,
            Name = baseVm.Name,
            Oab = baseVm.Oab,
            City = baseVm.City,
            Specialties = baseVm.Specialties,
            Bio = baseVm.Bio,
            Rating = baseVm.Rating,
            Online = baseVm.Online,
            Whatsapp = baseVm.Whatsapp,
            Email = baseVm.Email,
            Address = baseVm.Address,
            Availability = baseVm.Availability,
            Color = baseVm.Color,
            Initials = baseVm.Initials,
            Photo = baseVm.Photo,
            IsActive = baseVm.IsActive,
            SubsectionSlug = subsection.Slug,
            SubsectionName = subsection.Name,
            SubsectionPrimaryColor = subsection.PrimaryColor,
            SubsectionAccentColor = subsection.AccentColor,
            SubsectionLogoUrl = subsection.LogoUrl,
        };
    }

    public static HubArticleViewModel ToHubArticle(Article entity, Subsection subsection)
    {
        var baseVm = ArticleMapper.ToViewModel(entity);
        return new HubArticleViewModel
        {
            Id = baseVm.Id,
            Slug = baseVm.Slug,
            Title = baseVm.Title,
            Icon = baseVm.Icon,
            Summary = baseVm.Summary,
            Content = baseVm.Content,
            SubsectionSlug = subsection.Slug,
            SubsectionName = subsection.Name,
            SubsectionPrimaryColor = subsection.PrimaryColor,
            SubsectionAccentColor = subsection.AccentColor,
            SubsectionLogoUrl = subsection.LogoUrl,
        };
    }

    public static HubNotificationViewModel ToHubNotification(Notification entity, Subsection subsection)
    {
        var baseVm = NotificationMapper.ToViewModel(entity);
        return new HubNotificationViewModel
        {
            Id = baseVm.Id,
            Icon = baseVm.Icon,
            Text = baseVm.Text,
            Date = baseVm.Date,
            ForLawyer = baseVm.ForLawyer,
            Href = baseVm.Href,
            SubsectionSlug = subsection.Slug,
            SubsectionName = subsection.Name,
            SubsectionPrimaryColor = subsection.PrimaryColor,
            SubsectionAccentColor = subsection.AccentColor,
            SubsectionLogoUrl = subsection.LogoUrl,
        };
    }

    public static HubElectionViewModel ToHubElection(Election entity, Subsection subsection)
    {
        var baseVm = ElectionMapper.ToViewModel(entity);
        return new HubElectionViewModel
        {
            Id = baseVm.Id,
            Year = baseVm.Year,
            Term = baseVm.Term,
            EndDate = baseVm.EndDate,
            DaysRemaining = baseVm.DaysRemaining,
            PortalUrl = baseVm.PortalUrl,
            TotalSlates = baseVm.TotalSlates,
            Candidates = baseVm.Candidates,
            SubsectionSlug = subsection.Slug,
            SubsectionName = subsection.Name,
            SubsectionPrimaryColor = subsection.PrimaryColor,
            SubsectionAccentColor = subsection.AccentColor,
            SubsectionLogoUrl = subsection.LogoUrl,
        };
    }
}
