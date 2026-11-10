using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.Web.Controllers
{
    /// <summary>
    /// ავტორიზაციისა და რეგისტრაციის კონტროლერი. პასუხისმგებელია მომხმარებლებისა და 
    /// ადმინისტრატორების რეგისტრაციაზე, ელ-ფოსტის დადასტურებაზე და სისტემაში შემოსვლაზე.
    /// </summary>
    [AllowAnonymous]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _auth;

        /// <summary>
        /// ინიციალიზაციას უკეთებს AuthController კლასის ახალ ეგზემპლარს.
        /// </summary>
        /// <param name="auth">ავტორიზაციის სერვისი.</param>
        public AuthController(IAuthService auth) => _auth = auth;

        /// <summary>არეგისტრირებს ახალ მომხმარებელს და ელ-ფოსტით უგზავნის დადასტურების ბმულს. მომხმარებელს სისტემაში შესვლა მხოლოდ დადასტურების შემდეგ შეუფლია.</summary>
        /// <response code="201">რეგისტრაცია წარმატებულია; გაიგზავნა დადასტურების ელ-ფოსტა.</response>
        /// <response code="400">ვალიდაცია ვერ გაიარა (ელ-ფოსტის, სახელის ან პაროლის წესები).</response>
        /// <response code="409">ელ-ფოსტა ან მომხმარებლის სახელი დაკავებულია.</response>
        /// <response code="422">Identity-მ მონაცემები უარყო (მაგალითად, სუსტი პაროლი).</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
        {
            var result = await _auth.RegisterCustomerAsync(request, ct);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>არეგისტრირებს ახალ ადმინისტრატორს. საჭიროებს სერვერზე დაკონფიგურებულ AdminSecret-ს.</summary>
        /// <response code="201">რეგისტრაცია წარმატებულია; ადმინისტრატორის ელ-ფოსტა წინასწარ დადასტურებულია და ბრუნდება JWT.</response>
        /// <response code="400">ვალიდაცია ვერ გაიარა.</response>
        /// <response code="403">არასწორი ადმინისტრატორის სეკრეტი (AdminSecret).</response>
        /// <response code="409">ელ-ფოსტა ან მომხმარებლის სახელი დაკავებულია.</response>
        [HttpPost("register-admin")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminRequest request, CancellationToken ct)
        {
            var result = await _auth.RegisterAdminAsync(request, ct);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>დადასტურების ელ-ფოსტაში არსებული ბმულის სამიზნე მისამართი.</summary>
        /// <response code="200">ელ-ფოსტა დადასტურებულია.</response>
        /// <response code="422">ბმული არასწორია ან ვადაგასულია.</response>
        [HttpGet("confirm-email")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<MessageResponse>> ConfirmEmail([FromQuery] ConfirmEmailRequest request, CancellationToken ct)
        {
            await _auth.ConfirmEmailAsync(request, ct);
            return Ok(new MessageResponse("Your email has been confirmed. You can now log in."));
        }

        /// <summary>აგზავნის ახალ დადასტურების ელ-ფოსტას. ყოველთვის აბრუნებს 200-ს, რათა ვერ იქნას გამოყენებული რეგისტრირებული მისამართების გამოსავლენად.</summary>
        /// <response code="200">ყოველთვის, მიუხედავად იმისა, რეგისტრირებულია თუ არა მისამართი.</response>
        [HttpPost("resend-confirmation")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<MessageResponse>> ResendConfirmation([FromBody] ResendConfirmationRequest request, CancellationToken ct)
        {
            await _auth.ResendConfirmationAsync(request, ct);
            return Ok(new MessageResponse("If the address is registered and not yet confirmed, a new confirmation email has been sent."));
        }

        /// <summary>ახორციელებს სისტემაში შესვლას ელ-ფოსტითა და პაროლით და აბრუნებს JWT-ს (ელ-ფოსტა აუცილებლად უნდა იყოს დადასტურებული).</summary>
        /// <response code="200">JWT ტოკენი, მისი ვადის გასვლის დრო და მომხმარებლის როლები.</response>
        /// <response code="401">არასწორი ელ-ფოსტა ან პაროლი.</response>
        /// <response code="403">ელ-ფოსტა ჯერ კიდევ არ არის დადასტურებული.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
            => Ok(await _auth.LoginAsync(request, ct));
    }
}