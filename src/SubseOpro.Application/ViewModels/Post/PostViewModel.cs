namespace SubseOpro.Application.ViewModels.Post;

public class PostViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Photo { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public string? Link { get; set; }
    public string Source { get; set; } = "manual";
    public bool IsActive { get; set; }
    public int Likes { get; set; }
}
