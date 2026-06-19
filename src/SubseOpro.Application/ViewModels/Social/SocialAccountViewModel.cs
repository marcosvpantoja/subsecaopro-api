namespace SubseOpro.Application.ViewModels.Social;

public class SocialAccountViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string Handle { get; set; } = string.Empty;
    public string ProfileUrl { get; set; } = string.Empty;
    public bool Connected { get; set; }
    public int? FollowersCount { get; set; }
    public DateTime? LastSync { get; set; }
}
