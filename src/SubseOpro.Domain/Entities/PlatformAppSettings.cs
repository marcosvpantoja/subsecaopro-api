namespace SubseOpro.Domain.Entities;

/// <summary>
/// Configuração do app compartilhado na loja (feed multi-subseção, modo público).
/// Singleton — editável somente pelo sysadmin da plataforma.
/// </summary>
public class PlatformAppSettings
{
    public Guid Id { get; set; }
    public bool PublicAppEnabled { get; set; }
    public string PlatformName { get; set; } = "SubseçãoPRO";
    public string? DefaultSubsectionSlug { get; set; }
    /// <summary>mock | global | per_subsection | off</summary>
    public string AiMode { get; set; } = "mock";
    public string? AiGlobalPrompt { get; set; }
    public string? AiDisclaimer { get; set; }
    /// <summary>JSON serializado de PublicModulesConfig (Application layer).</summary>
    public string PublicModulesJson { get; set; } = "{}";
}
