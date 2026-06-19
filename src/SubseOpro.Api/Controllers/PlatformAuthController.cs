using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubseOpro.Api.Services;
using SubseOpro.Application.Mappers;
using SubseOpro.Application.Services;
using SubseOpro.Application.ViewModels.Auth;
using SubseOpro.Domain.Interfaces;

namespace SubseOpro.Api.Controllers;

[ApiController]
[Route("api/v1/platform/auth")]
public class PlatformAuthController : ControllerBase
{
    private readonly IRepositoryFactory _factory;
    private readonly JwtService _jwtService;

    public PlatformAuthController(IRepositoryFactory factory, JwtService jwtService)
    {
        _factory = factory;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseViewModel>> Login(
        [FromBody] PlatformLoginRequestViewModel request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { detail = "E-mail e senha são obrigatórios." });

        var isValid = await _factory.PlatformAdmins.ValidateCredentialsAsync(request.Email.Trim(), request.Password, ct);
        if (!isValid)
            return Unauthorized(new { detail = "Credenciais inválidas." });

        var admin = await _factory.PlatformAdmins.GetByEmailAsync(request.Email.Trim(), ct);
        if (admin is null || !admin.IsActive)
            return Unauthorized(new { detail = "Credenciais inválidas." });

        return Ok(new LoginResponseViewModel
        {
            Token = _jwtService.GeneratePlatformAdminToken(admin),
            User = new AuthUserViewModel
            {
                Id = admin.Id.ToString(),
                Name = admin.Name,
                Email = admin.Email,
                Roles = ["platform_admin"],
            },
        });
    }
}

public class PlatformLoginRequestViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
