using SubseOpro.Domain.Enums;

namespace SubseOpro.Domain.Entities;

public class Post
{
    public Guid Id { get; set; }
    public Guid SubsectionId { get; set; }
    public Subsection Subsection { get; set; } = null!;
    public DateTime PublishedAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ImageGradient { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public PostSource Source { get; set; } = PostSource.Manual;
    public bool IsActive { get; set; } = true;
    public string Caption { get; set; } = string.Empty;
    public int LikesCount { get; set; }
}
