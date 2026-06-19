namespace SubseOpro.Application.ViewModels.Banner;

public class BannerViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Cta { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Photo { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public Dictionary<string, string>? Params { get; set; }
}
