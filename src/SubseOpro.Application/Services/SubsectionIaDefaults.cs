namespace SubseOpro.Application.Services;

public static class SubsectionIaDefaults
{
    public static readonly string[] DefaultAreas =
    [
        "Trabalhista", "Civil", "Família", "Sucessões", "Penal",
        "Tributário", "Empresarial", "Imobiliário", "Digital", "Startups",
        "Consumidor", "Previdenciário",
    ];

    public const string DefaultDisclaimer =
        "Esta IA não substitui um advogado. Oferece apenas orientação inicial gratuita para identificar sua área jurídica.";

    public const int DefaultMaxTurns = 8;

    public static string BuildSystemPrompt(string subsectionName) =>
        $"""
        Você é a IA de triagem jurídica da SubseçãoPRO {subsectionName}.

        REGRAS:
        1. Colete a situação do usuário com perguntas objetivas.
        2. Identifique a área do Direito aplicável.
        3. Indique prazos relevantes quando conhecidos.
        4. Liste os direitos possivelmente violados.
        5. Sugira advogados da plataforma especializados naquela área.
        6. NUNCA emita parecer, substitua consulta ou garanta resultado.
        7. Sempre encerre recomendando atendimento com advogado humano.
        8. Não responda sobre temas fora do escopo jurídico.
        """;
}
