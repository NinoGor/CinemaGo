using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;

namespace CinemaGo.Domain.Interfaces;

public interface ISessionRepository : IRepository<MovieSession>
{
    Task<MovieSession?> GetWithDetailsAsync(int id, CancellationToken ct = default);

    Task<PagedResult<MovieSession>> SearchAsync(SessionFilter filter, CancellationToken ct = default);

    // თუ არის დაგეგმილი სესია დარბაზში, რომლის დროის ინტერვალი გადაკვეთს [start, end) ინტერვალს
    Task<bool> HasOverlapAsync(int hallId, DateTime start, DateTime end, int? excludeSessionId, CancellationToken ct = default);
}
