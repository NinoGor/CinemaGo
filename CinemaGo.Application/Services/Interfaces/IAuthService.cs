using CinemaGo.Domain.Models.DTOs;
namespace CinemaGo.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResponse> RegisterCustomerAsync(RegisterRequest request, CancellationToken ct = default);
        Task ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken ct = default);
        Task ResendConfirmationAsync(ResendConfirmationRequest request, CancellationToken ct = default);
        Task<AuthResponse> RegisterAdminAsync(RegisterAdminRequest request, CancellationToken ct = default);
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    }
}
