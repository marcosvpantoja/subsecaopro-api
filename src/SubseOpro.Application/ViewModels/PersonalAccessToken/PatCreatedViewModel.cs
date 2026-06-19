namespace SubseOpro.Application.ViewModels.PersonalAccessToken;

public class PatCreatedViewModel
{
    public PatViewModel Pat { get; set; } = new();
    public string Token { get; set; } = string.Empty;
    public string? EncryptedToken { get; set; }
}
