using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubseOpro.Api.Services;
using SubseOpro.Application.Mappers;
using SubseOpro.Application.Services;
using SubseOpro.Application.ViewModels.Auth;
using SubseOpro.Domain.Interfaces;

namespace SubseOpro.Api.Controllers;

/// <summary>
/// Endpoints de autenticação, sessão e credenciais.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IRepositoryFactory _factory;
    private readonly JwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordResetService _passwordResetService;

    public AuthController(
        IRepositoryFactory factory,
        JwtService jwtService,
        IPasswordHasher passwordHasher,
        IPasswordResetService passwordResetService)
    {
        _factory = factory;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _passwordResetService = passwordResetService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseViewModel>> Login(
        [FromBody] LoginRequestViewModel request, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(request.Password))
            return BadRequest(new { detail = "Senha obrigatória." });

        var oab = request.Oab?.Trim();
        if (!string.IsNullOrWhiteSpace(oab))
        {
            var lawyer = await _factory.Lawyers.GetByOabNumberGlobalAsync(oab, ct);
            if (lawyer is null || !lawyer.IsActive || string.IsNullOrEmpty(lawyer.PasswordHash))
                return Unauthorized(new { detail = "OAB ou senha inválidos." });

            if (!_passwordHasher.Verify(request.Password, lawyer.PasswordHash))
                return Unauthorized(new { detail = "OAB ou senha inválidos." });

            var subsection = await _factory.Subsections.GetByIdAsync(lawyer.SubsectionId, ct);
            var userVm = LawyerMapper.ToAuthUserViewModel(lawyer);
            if (subsection is not null)
                AuthMapper.EnrichWithSubsection(userVm, subsection);

            return Ok(new LoginResponseViewModel
            {
                Token = _jwtService.GenerateLawyerToken(lawyer),
                User = userVm,
            });
        }

        var email = request.Email?.Trim();
        if (!string.IsNullOrWhiteSpace(email))
        {
            var admin = await _factory.AdminUsers.GetByEmailGlobalAsync(email, ct);
            if (admin is null || !admin.IsActive)
                return Unauthorized(new { detail = "Credenciais inválidas." });

            if (!_passwordHasher.Verify(request.Password, admin.PasswordHash))
                return Unauthorized(new { detail = "Credenciais inválidas." });

            var subsection = await _factory.Subsections.GetByIdAsync(admin.SubsectionId, ct);
            var userVm = AdminMapper.ToAuthUserViewModel(admin);
            if (subsection is not null)
                AuthMapper.EnrichWithSubsection(userVm, subsection);

            return Ok(new LoginResponseViewModel
            {
                Token = _jwtService.GenerateBackofficeToken(admin),
                User = userVm,
            });
        }

        return BadRequest(new { detail = "Informe OAB ou e-mail para autenticar." });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequestViewModel request,
        CancellationToken ct)
    {
        await _passwordResetService.RequestResetAsync(request.Email, ct);
        return NoContent();
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequestViewModel request,
        CancellationToken ct)
    {
        var success = await _passwordResetService.ResetPasswordAsync(
            request.Email,
            request.Code,
            request.NewPassword,
            ct);

        if (!success)
            return BadRequest(new { detail = "Código inválido ou expirado." });

        return NoContent();
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordViewModel request, CancellationToken ct)
    {
        if (!PasswordPolicy.IsValid(request.NewPassword, out var policyError))
            return BadRequest(new { detail = policyError });

        if (request.CurrentPassword == request.NewPassword)
            return BadRequest(new { detail = "A nova senha deve ser diferente da atual." });

        var lawyerId = GetSubjectId();
        if (lawyerId is null) return Unauthorized();

        var lawyer = await _factory.Lawyers.GetByIdAsync(lawyerId.Value, ct);
        if (lawyer is null) return NotFound();

        if (string.IsNullOrEmpty(lawyer.PasswordHash) ||
            !_passwordHasher.Verify(request.CurrentPassword, lawyer.PasswordHash))
            return Unauthorized(new { detail = "Senha atual incorreta." });

        var newHash = _passwordHasher.Hash(request.NewPassword);
        await _factory.Lawyers.UpdatePasswordHashAsync(lawyerId.Value, newHash, ct);

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthUserViewModel>> Me(CancellationToken ct)
    {
        var subjectId = GetSubjectId();
        if (subjectId is null) return Unauthorized();

        var accountType = User.FindFirstValue("account_type");
        if (accountType == "backoffice")
        {
            var admin = await _factory.AdminUsers.GetByIdAsync(subjectId.Value, ct);
            if (admin is null) return NotFound();
            var vm = AdminMapper.ToAuthUserViewModel(admin);
            var subsection = await _factory.Subsections.GetByIdAsync(admin.SubsectionId, ct);
            if (subsection is not null)
                AuthMapper.EnrichWithSubsection(vm, subsection);
            return Ok(vm);
        }

        if (accountType == "platform_admin")
        {
            var platformAdmin = await _factory.PlatformAdmins.GetByIdAsync(subjectId.Value, ct);
            if (platformAdmin is null) return NotFound();
            return Ok(new AuthUserViewModel
            {
                Id = platformAdmin.Id.ToString(),
                Name = platformAdmin.Name,
                Email = platformAdmin.Email,
                Roles = ["platform_admin"],
                AccountType = "platform_admin",
            });
        }

        var lawyer = await _factory.Lawyers.GetByIdAsync(subjectId.Value, ct);
        if (lawyer is null) return NotFound();
        var lawyerVm = LawyerMapper.ToAuthUserViewModel(lawyer);
        var lawyerSubsection = await _factory.Subsections.GetByIdAsync(lawyer.SubsectionId, ct);
        if (lawyerSubsection is not null)
            AuthMapper.EnrichWithSubsection(lawyerVm, lawyerSubsection);
        return Ok(lawyerVm);
    }

    private Guid? GetSubjectId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
