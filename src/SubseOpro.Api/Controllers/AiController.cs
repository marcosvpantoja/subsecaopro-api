using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubseOpro.Domain.Interfaces;

namespace SubseOpro.Api.Controllers;

[ApiController]
[Route("api/v1/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IRepositoryFactory _factory;
    private readonly IHttpClientFactory _httpClientFactory;

    public AiController(IRepositoryFactory factory, IHttpClientFactory httpClientFactory)
    {
        _factory = factory;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("triagem/inicio")]
    public Task<IActionResult> TriagemInicio(CancellationToken ct) =>
        ProxyGetAsync("triagem/inicio", ct);

    [HttpPost("triagem/mensagem")]
    public Task<IActionResult> TriagemMensagem(CancellationToken ct) =>
        ProxyPostAsync("triagem/mensagem", ct);

    [HttpPost("triagem/atendimento")]
    public Task<IActionResult> TriagemAtendimento(CancellationToken ct) =>
        ProxyPostAsync("triagem/atendimento", ct);

    private async Task<IActionResult> ProxyGetAsync(string path, CancellationToken ct)
    {
        var aiUrl = await GetAiServiceUrlAsync(ct);
        if (aiUrl is null) return StatusCode(503, new { detail = "Serviço de IA não configurado para esta subseção." });

        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync($"{aiUrl.TrimEnd('/')}/{path}", ct);

        return await BuildProxyResultAsync(response, ct);
    }

    private async Task<IActionResult> ProxyPostAsync(string path, CancellationToken ct)
    {
        var aiUrl = await GetAiServiceUrlAsync(ct);
        if (aiUrl is null) return StatusCode(503, new { detail = "Serviço de IA não configurado para esta subseção." });

        var client = _httpClientFactory.CreateClient();
        var content = new StreamContent(Request.Body);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var response = await client.PostAsync($"{aiUrl.TrimEnd('/')}/{path}", content, ct);

        return await BuildProxyResultAsync(response, ct);
    }

    private async Task<string?> GetAiServiceUrlAsync(CancellationToken ct)
    {
        var subsection = await _factory.Subsections.GetCurrentAsync(ct);
        return string.IsNullOrWhiteSpace(subsection?.AiServiceUrl) ? null : subsection.AiServiceUrl;
    }

    private static async Task<IActionResult> BuildProxyResultAsync(
        HttpResponseMessage response,
        CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);
        return new ContentResult
        {
            StatusCode = (int)response.StatusCode,
            Content = body,
            ContentType = "application/json",
        };
    }
}
