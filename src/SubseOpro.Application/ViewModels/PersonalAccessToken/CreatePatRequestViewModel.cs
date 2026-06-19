namespace SubseOpro.Application.ViewModels.PersonalAccessToken;

public class CreatePatRequestViewModel
{
    public string Name { get; set; } = string.Empty;
    public Guid SubsectionId { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool SetAsActiveAiToken { get; set; }
}
