namespace SubseOpro.Domain.Entities;

public class SocialAccount
{
    public Guid Id { get; set; }
    public Guid SubsectionId { get; set; }
    public Subsection Subsection { get; set; } = null!;
    public string Platform { get; set; } = string.Empty;
    public string Handle { get; set; } = string.Empty;
    public string ProfileUrl { get; set; } = string.Empty;
    public bool Connected { get; set; }
    public int? FollowersCount { get; set; }
    public DateTime? LastSync { get; set; }
}
