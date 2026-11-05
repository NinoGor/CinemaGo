using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.Web.Controllers;

[AllowAnonymous]
public class AuthController : BaseApiController
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    // არეგისტრირებს ახალ მომხმარებელს და უგზავნის დადასტურების ბმულს ელ-ფოსტაზე. მომხმარებელი შეძლებს შესვლას მხოლოდ დადასტურების შემდეგ.
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _auth.RegisterCustomerAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // არეგისტრირებს ახალ ადმინს, საჭიროებს სერვერზე დაკონფიგურებულ AdminSecret-ს
    [HttpPost("register-admin")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminRequest request, CancellationToken ct)
    {
        var result = await _auth.RegisterAdminAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("confirm-email")]
    public async Task<ActionResult<MessageResponse>> ConfirmEmail([FromQuery] ConfirmEmailRequest request, CancellationToken ct)
    {
        await _auth.ConfirmEmailAsync(request, ct);
        return Ok(new MessageResponse("Your email has been confirmed. You can now log in."));
    }

    [HttpPost("resend-confirmation")]
    public async Task<ActionResult<MessageResponse>> ResendConfirmation([FromBody] ResendConfirmationRequest request, CancellationToken ct)
    {
        await _auth.ResendConfirmationAsync(request, ct);
        return Ok(new MessageResponse("If the address is registered and not yet confirmed, a new confirmation email has been sent."));
    }

    // შესვლა ელ-ფოსტით და პაროლით და JWT-ის დაბრუნება (ელ-ფოსტა თუ დადასტურებულია)
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
        => Ok(await _auth.LoginAsync(request, ct));
}
