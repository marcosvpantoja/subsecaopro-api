namespace SubseOpro.Application.ViewModels.Lawyer;

public class LawyerUpdateViewModel
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Whatsapp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Availability { get; set; } = string.Empty;
    public bool Online { get; set; }
    public string Photo { get; set; } = string.Empty;
    public List<string> Specialties { get; set; } = [];
}
