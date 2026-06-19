using SubseOpro.Application.ViewModels.Article;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Application.Mappers;

public static class ArticleMapper
{
    public static ArticleViewModel ToViewModel(Article entity)
    {
        return new ArticleViewModel
        {
            Id = entity.Id.ToString(),
            Slug = entity.Slug,
            Title = entity.Title,
            Icon = entity.Icon,
            Summary = entity.Summary,
            Content = entity.Content ?? string.Empty,
        };
    }

    public static List<ArticleViewModel> ToViewModelList(IEnumerable<Article> entities)
        => entities.Select(ToViewModel).ToList();
}
