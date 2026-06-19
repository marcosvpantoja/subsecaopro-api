namespace SubseOpro.Domain.Entities;

public class Banner
{
    public Guid Id { get; set; }
    public Guid SubsectionId { get; set; }
    public Subsection Subsection { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Cta { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string LinkTo { get; set; } = string.Empty;
    public Dictionary<string, string>? LinkParams { get; set; }
    public int SortOrder { get; set; }
}
