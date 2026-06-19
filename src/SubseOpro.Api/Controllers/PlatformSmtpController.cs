using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubseOpro.Application.Services;
using SubseOpro.Application.ViewModels.Platform;

namespace SubseOpro.Api.Controllers;

[ApiController]
[Route("api/v1/platform/smtp")]
[Authorize(Policy = "PlatformAdmin")]
public class PlatformSmtpController : ControllerBase
{
    private readonly ISmtpConfigurationRepository _smtpConfigurationRepository;

    public PlatformSmtpController(ISmtpConfigurationRepository smtpConfigurationRepository)
    {
        _smtpConfigurationRepository = smtpConfigurationRepository;
    }

    [HttpGet("config")]
    public async Task<ActionResult<SmtpConfigViewModel>> GetConfig(CancellationToken cancellationToken = default)
    {
        var current = await _smtpConfigurationRepository.GetAsync(cancellationToken);
        return Ok(new SmtpConfigViewModel
        {
            Host = current.Host,
            Port = current.Port,
            Username = current.Username,
            Password = current.Password,
            FromEmail = current.FromEmail,
            FromName = current.FromName,
            UseTls = current.UseTls,
            IsActive = current.IsActive,
        });
    }

    [HttpPut("config")]
    public async Task<ActionResult<SmtpSaveViewModel>> SaveConfig(
        [FromBody] SaveSmtpConfigRequestViewModel request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _smtpConfigurationRepository.SaveAsync(
                new SaveSmtpConfigurationInput(
                    request.Host,
                    request.Port,
                    request.Username,
                    request.Password,
                    request.FromEmail,
                    request.FromName,
                    request.UseTls,
                    request.IsActive),
                cancellationToken);
            return Ok(new SmtpSaveViewModel { Saved = result.Saved, UpdatedAt = result.UpdatedAt });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { detail = ex.Message });
        }
    }

    [HttpPost("test")]
    public async Task<ActionResult<SmtpTestViewModel>> SendTest(
        [FromBody] SendSmtpTestRequestViewModel request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _smtpConfigurationRepository.SendTestAsync(
                new SendSmtpTestInput(request.ToEmail),
                cancellationToken);
            return Ok(new SmtpTestViewModel { Sent = result.Sent, ToEmail = result.ToEmail });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { detail = ex.Message });
        }
        catch (TimeoutException ex)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { detail = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { detail = ex.Message });
        }
    }

    [HttpPost("validate-connection")]
    public async Task<ActionResult<SmtpConnectionViewModel>> ValidateConnection(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _smtpConfigurationRepository.ValidateConnectionAsync(cancellationToken);
            return Ok(new SmtpConnectionViewModel
            {
                Connected = result.Connected,
                Host = result.Host,
                Port = result.Port,
                UseTls = result.UseTls,
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { detail = ex.Message });
        }
        catch (TimeoutException ex)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { detail = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { detail = ex.Message });
        }
    }
}
