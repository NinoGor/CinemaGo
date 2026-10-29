using CinemaGo.Domain.Models.DTOs;

namespace CinemaGo.Application.Services.Interfaces
{
    public interface ISessionService
    {
        Task<PagedResult<SessionResponse>> SearchAsync(SessionFilter filter, CancellationToken ct = default);
        Task<SessionResponse> GetByIdAsync(int id, CancellationToken ct = default);
        Task<SessionSeatsResponse> GetSeatsAsync(int sessionId, CancellationToken ct = default);
        Task<SessionResponse> CreateAsync(SessionRequest request, CancellationToken ct = default);
        Task<SessionResponse> UpdateAsync(int id, SessionRequest request, CancellationToken ct = default);

        // აუქმებს სესიას, აუქმებს აქტიურ ჯავშნებს და აგზავნის მეილებს
        Task<SessionCancellationResponse> CancelAsync(int id, CancellationToken ct = default);
    }
}
