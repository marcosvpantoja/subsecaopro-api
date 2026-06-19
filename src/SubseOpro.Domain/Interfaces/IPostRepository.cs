using SubseOpro.Domain.Entities;

namespace SubseOpro.Domain.Interfaces;

public interface IPostRepository : IRepository<Post>
{
    Task<IReadOnlyList<Post>> GetRecentAsync(CancellationToken ct = default);
    Task<Post> LikePostAsync(Guid postId, CancellationToken ct = default);
    Task<Post?> GetByIdNoTrackingAsync(Guid postId, CancellationToken ct = default);
}
