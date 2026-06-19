using System.Net;
using System.Text.Json;

namespace SubseOpro.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operação inválida");
            await WriteProblem(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno");
            await WriteProblem(context, HttpStatusCode.InternalServerError, "Erro interno do servidor.");
        }
    }

    private static async Task WriteProblem(HttpContext context, HttpStatusCode status, string detail)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;

        var problem = new
        {
            status = (int)status,
            title = status.ToString(),
            detail,
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
