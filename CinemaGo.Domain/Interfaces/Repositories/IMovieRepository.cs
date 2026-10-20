using CinemaGo.Domain.Models.Entities;
namespace CinemaGo.Domain.Interfaces.Repositories
{
    public interface IMovieRepository : IRepository<Movie>
    {
        Task<IReadOnlyList<Movie>> GetFilteredAsync(string? title, int? genreId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> CountFilteredAsync(string? title, int? genreId, CancellationToken cancellationToken = default);
        Task<Movie?> GetWithGenresByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
