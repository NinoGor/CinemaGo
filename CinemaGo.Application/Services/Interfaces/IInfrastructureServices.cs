using CinemaGo.Domain.Models.Entities;

namespace CinemaGo.Application.Services.Interfaces
{
    // აბსტრაქცია ტესტირებისთვის
    public interface IClock
    {
        DateTime UtcNow { get; }
    }

    public interface ITicketCodeGenerator
    {
        string Generate();
    }

    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
    }

    public interface IJwtTokenGenerator
    {
        (string Token, DateTime ExpiresAt) Generate(ApplicationUser user, IEnumerable<string> roles);
    }
}
