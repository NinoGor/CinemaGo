using System.Net;
using System.Security.Cryptography;
using System.Text;
using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Application.Settings;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Helpers;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CinemaGo.Application.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly IEmailService _email;
        private readonly AdminRegistrationSettings _adminSettings;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IJwtTokenGenerator tokenGenerator,
            IEmailService email,
            IOptions<AdminRegistrationSettings> adminSettings,
            IOptions<EmailSettings> emailSettings,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenGenerator = tokenGenerator;
            _email = email;
            _adminSettings = adminSettings.Value;
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<RegisterResponse> RegisterCustomerAsync(RegisterRequest request, CancellationToken ct = default)
        {
            var user = await CreateUserAsync(
                request.UserName, request.Email, request.Password, RoleNames.Customer, emailConfirmed: false);

            var sent = await TrySendConfirmationEmailAsync(user, ct);

            var message = sent
                ? "Registration successful. Please check your email and click the confirmation link before logging in."
                : "Registration successful, but the confirmation email could not be sent. " +
                  "Use POST /api/auth/resend-confirmation to try again.";

            return new RegisterResponse(user.Id, user.Email ?? string.Empty, message);
        }

        public async Task<AuthResponse> RegisterAdminAsync(RegisterAdminRequest request, CancellationToken ct = default)
        {
            if (!IsValidAdminSecret(request.AdminSecret))
            {
                _logger.LogWarning("Admin registration rejected for {Email}: invalid secret key.", request.Email);
                throw new ForbiddenException("Invalid admin registration key.");
            }

            // გამომძახებელი ამტკიცებს server-side secret-ის ფლობას, ამიტომ ადმინი პირდაპირ ვერიფიცირებულია
            var user = await CreateUserAsync(
                request.UserName, request.Email, request.Password, RoleNames.Admin, emailConfirmed: true);

            return BuildResponse(user, new[] { RoleNames.Admin });
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                _logger.LogWarning("Failed login attempt for {Email}.", request.Email);
                throw new UnauthorizedException("Invalid email or password.");
            }

            if (!user.EmailConfirmed)
                throw new ForbiddenException(
                    "Please confirm your email address before logging in. " +
                    "You can request a new link with POST /api/auth/resend-confirmation.");

            var roles = await _userManager.GetRolesAsync(user);
            return BuildResponse(user, roles);
        }

        public async Task ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString())
                       ?? throw new BusinessRuleException("The confirmation link is invalid.");

            if (user.EmailConfirmed) return; 

            var result = await _userManager.ConfirmEmailAsync(user, request.Token);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Email confirmation failed for user {UserId}.", user.Id);
                throw new BusinessRuleException("The confirmation link is invalid or has expired. Please request a new one.");
            }

            _logger.LogInformation("User {UserId} confirmed their email.", user.Id);
        }

        public async Task ResendConfirmationAsync(ResendConfirmationRequest request, CancellationToken ct = default)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null || user.EmailConfirmed) return;

            await TrySendConfirmationEmailAsync(user, ct);
        }

        // ------------------------------------------------------------------ helpers
        private async Task<ApplicationUser> CreateUserAsync(
            string userName, string email, string password, string role, bool emailConfirmed)
        {
            if (await _userManager.FindByEmailAsync(email) is not null)
                throw new ConflictException("A user with this email already exists.");

            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new ApplicationRole(role));

            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = emailConfirmed,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, password); // Identity ჰეშავს
            EnsureSucceeded(createResult);

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            EnsureSucceeded(roleResult);

            _logger.LogInformation("User {UserId} registered with role {Role}.", user.Id, role);
            return user;
        }

        // მეილ-სერვერის პრობლემამ არ უნდა დაა-fail-ოს რეგისტრაცია, მომხმარებელს უნდა შეეძლოს მეილის ხელახალი მოთხოვნა 
        private async Task<bool> TrySendConfirmationEmailAsync(ApplicationUser user, CancellationToken ct)
        {
            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var link = $"{_emailSettings.PublicBaseUrl.TrimEnd('/')}/api/auth/confirm-email" +
                           $"?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                // Notice the '$$' at the start: single {} are CSS, double {{}} are C# variables
                var body = $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <style>
                    body { font-family: Arial, sans-serif; background-color: #f4f4f5; margin: 0; padding: 0; }
                    .container { max-width: 600px; margin: 30px auto; background: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.05); }
                    .header { background: #e50914; color: #ffffff; padding: 20px; text-align: center; font-size: 24px; font-weight: bold; }
                    .content { padding: 30px; color: #333333; line-height: 1.6; }
                    .btn { display: inline-block; background: #e50914; color: #ffffff !important; text-decoration: none; padding: 12px 24px; border-radius: 4px; font-weight: bold; margin: 20px 0; }
                    .footer { background: #f9f9f9; padding: 15px; text-align: center; font-size: 12px; color: #777777; }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="header">CinemaGo</div>
                    <div class="content">
                        <p>Hello <strong>{{WebUtility.HtmlEncode(user.UserName)}}</strong>,</p>
                        <p>Welcome to CinemaGo! We're thrilled to have you. Please confirm your email address to activate your account and start booking movies:</p>
                        <p style="text-align: center;">
                            <a href="{{WebUtility.HtmlEncode(link)}}" class="btn">Confirm My Email</a>
                        </p>
                        <p>If the button above doesn't work, copy and paste this link into your web browser:</p>
                        <p style="word-break: break-all; font-size: 13px; color: #555;">{{WebUtility.HtmlEncode(link)}}</p>
                        <p>If you did not create this account, you can safely ignore this message.</p>
                    </div>
                    <div class="footer">
                        &copy; {{DateTime.UtcNow.Year}} CinemaGo. All rights reserved.
                    </div>
                </div>
            </body>
            </html>
            """;

                await _email.SendAsync(user.Email!, "Confirm your CinemaGo email", body, ct);

                if (string.Equals(_emailSettings.Provider, "Simulated", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("[DEV] Email confirmation link for {Email}: {Link}", user.Email, link);
                }

                return true;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Could not send the confirmation email to user {UserId}.", user.Id);
                return false;
            }
        }

        private AuthResponse BuildResponse(ApplicationUser user, IEnumerable<string> roles)
        {
            var roleList = roles.ToList();
            var (token, expiresAt) = _tokenGenerator.Generate(user, roleList);
            return new AuthResponse(user.Id, user.UserName ?? string.Empty, user.Email ?? string.Empty, roleList, token, expiresAt);
        }

        private bool IsValidAdminSecret(string? provided)
        {
            if (string.IsNullOrWhiteSpace(_adminSettings.SecretKey) ||
                _adminSettings.SecretKey.StartsWith("[ENTER") ||
                string.IsNullOrEmpty(provided))
                return false;

            var expectedBytes = Encoding.UTF8.GetBytes(_adminSettings.SecretKey);
            var providedBytes = Encoding.UTF8.GetBytes(provided);
            // დავიხმარე AI
            return CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
        }

        private static void EnsureSucceeded(IdentityResult result)
        {
            if (result.Succeeded) return;

            var message = string.Join(" ", result.Errors.Select(e => e.Description));
            if (result.Errors.Any(e => e.Code.StartsWith("Duplicate", StringComparison.Ordinal)))
                throw new ConflictException(message);

            throw new BusinessRuleException(message);
        }
    }
}
