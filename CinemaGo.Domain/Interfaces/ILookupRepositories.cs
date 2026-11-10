using CinemaGo.Domain.Models.Entities;

namespace CinemaGo.Domain.Interfaces
{
    public interface IGenreRepository : IRepository<Genre>
    {
        Task<IReadOnlyList<Genre>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
        Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
        Task<bool> IsUsedByMoviesAsync(int genreId, CancellationToken ct = default);

    }

    public interface IHallRepository : IRepository<Hall>
    {
        Task<IReadOnlyList<Hall>> GetAllWithSeatsAsync(CancellationToken ct = default);
        Task<Hall?> GetWithSeatsAsync(int id, CancellationToken ct = default);   // tracked
        Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
        Task<bool> HasSessionsAsync(int hallId, CancellationToken ct = default);
    }

    public interface ISeatRepository : IRepository<Seat>
    {
        Task<IReadOnlyList<Seat>> GetByHallAsync(int hallId, CancellationToken ct = default);
        Task<IReadOnlyList<Seat>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }
}
