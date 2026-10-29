using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;

namespace CinemaGo.Domain.Interfaces
{
    public interface IMovieRepository : IRepository<Movie>
    {
        Task<Movie?> GetWithGenresAsync(int id, CancellationToken ct = default);

        Task<PagedResult<Movie>> SearchAsync(MovieFilter filter, CancellationToken ct = default);

        Task<bool> HasSessionsAsync(int movieId, CancellationToken ct = default);
    }
}
