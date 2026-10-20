using CinemaGo.Domain.Models.DTOs;

namespace CinemaGo.Application.Services.Interfaces
{
    public interface ISessionService : IEntityService<SessionDto, SessionWriteDto>
    {
        Task<PagedResult<SessionDto>> SearchAsync(int? movieId, DateTime? startsFrom, DateTime? startsTo, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SeatAvailabilityDto>?> GetSeatsAsync(int sessionId, CancellationToken cancellationToken = default);
    }
}
