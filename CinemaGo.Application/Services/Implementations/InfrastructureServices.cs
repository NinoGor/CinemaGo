using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Application.Settings;
using CinemaGo.Domain.Models.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CinemaGo.Application.Services.Implementations
{
    public class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    public class TicketCodeGenerator : ITicketCodeGenerator
    {
        // "CG-" + 12 random hex სიმბოლო (48 bits) ასევე unique index განაპირობებს უნიკალურობას
        public string Generate() => "CG-" + Convert.ToHexString(RandomNumberGenerator.GetBytes(6));
    }

    public class SimulatedEmailService : IEmailService
    {
        private readonly ILogger<SimulatedEmailService> _logger;

        public SimulatedEmailService(ILogger<SimulatedEmailService> logger) => _logger = logger;

        public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
        {
            _logger.LogInformation("[SIMULATED EMAIL] To: {To} | Subject: {Subject} | Body: {Body}", to, subject, body);
            return Task.CompletedTask;
        }
    }

    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtSettings _settings;

        public JwtTokenGenerator(IOptions<JwtSettings> settings) => _settings = settings.Value;

        public (string Token, DateTime ExpiresAt) Generate(ApplicationUser user, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new(AppClaimTypes.UserId, user.Id.ToString()),
                new(AppClaimTypes.UserName, user.UserName ?? string.Empty),
                new(AppClaimTypes.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            claims.AddRange(roles.Select(role => new Claim(AppClaimTypes.Role, role)));

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
                SecurityAlgorithms.HmacSha256);

            var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}
