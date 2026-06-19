using SubseOpro.Application.ViewModels.Post;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Enums;

namespace SubseOpro.Application.Mappers;

public static class PostMapper
{
    public static PostViewModel ToViewModel(Post entity)
    {
        return new PostViewModel
        {
            Id = entity.Id.ToString(),
            Title = string.IsNullOrWhiteSpace(entity.Title) ? entity.Caption : entity.Title,
            Date = FormatRelativeDate(entity.PublishedAt),
            Image = entity.ImageGradient,
            Photo = entity.PhotoUrl,
            Caption = entity.Caption,
            Link = entity.LinkUrl,
            Source = ToSourceString(entity.Source),
            IsActive = entity.IsActive,
            Likes = entity.LikesCount,
        };
    }

    public static List<PostViewModel> ToViewModelList(IEnumerable<Post> entities)
        => entities.Select(ToViewModel).ToList();

    private static string FormatRelativeDate(DateTime publishedAt)
    {
        var diff = DateTime.UtcNow - publishedAt;
        if (diff.TotalHours <= 24) return "há 1h";
        if (diff.TotalDays <= 2) return "há 2 dias";
        if (diff.TotalDays <= 5) return "há 4 dias";
        if (diff.TotalDays <= 7) return "há 6 dias";
        if (diff.TotalDays <= 14) return "há 1 semana";
        if (diff.TotalDays <= 21) return "há 2 semanas";
        return "há 3 semanas";
    }

    public static PostSource ParseSource(string? source)
    {
        return source?.ToLowerInvariant() switch
        {
            "instagram" => PostSource.Instagram,
            "facebook" => PostSource.Facebook,
            "x" => PostSource.X,
            "linkedin" => PostSource.Linkedin,
            "youtube" => PostSource.Youtube,
            _ => PostSource.Manual,
        };
    }

    private static string ToSourceString(PostSource source)
    {
        return source switch
        {
            PostSource.Instagram => "instagram",
            PostSource.Facebook => "facebook",
            PostSource.X => "x",
            PostSource.Linkedin => "linkedin",
            PostSource.Youtube => "youtube",
            _ => "manual",
        };
    }
}
