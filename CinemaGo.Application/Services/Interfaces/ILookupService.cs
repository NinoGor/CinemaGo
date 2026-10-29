using CinemaGo.Domain.Models.DTOs;

namespace CinemaGo.Application.Services.Interfaces
{
    public interface ILookupService
    {
        Task<IReadOnlyList<GenreResponse>> GetGenresAsync(CancellationToken ct = default);
        Task<IReadOnlyList<HallResponse>> GetHallsAsync(CancellationToken ct = default);
    }
}
