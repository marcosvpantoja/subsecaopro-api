namespace SubseOpro.Domain.Entities;

public class Article
{
    public Guid Id { get; set; }
    public Guid SubsectionId { get; set; }
    public Subsection Subsection { get; set; } = null!;
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Content { get; set; }
}
