namespace SubseOpro.Application.ViewModels.Lawyer;

public class LawyerViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Oab { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public List<string> Specialties { get; set; } = [];
    public string Bio { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public bool Online { get; set; }
    public string Whatsapp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Availability { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Photo { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
