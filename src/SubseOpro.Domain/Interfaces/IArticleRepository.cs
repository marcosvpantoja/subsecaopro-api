using SubseOpro.Domain.Entities;

namespace SubseOpro.Domain.Interfaces;

public interface IArticleRepository : IRepository<Article>
{
    Task<Article?> GetBySlugAsync(string slug, CancellationToken ct = default);
}
