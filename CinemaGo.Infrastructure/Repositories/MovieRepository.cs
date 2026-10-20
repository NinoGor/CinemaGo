using CinemaGo.Domain.Interfaces.Repositories; 
using CinemaGo.Domain.Models.Entities; 
using CinemaGo.Infrastructure.Data; 
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Infrastructure.Repositories
{
    public sealed class MovieRepository : Repository<Movie>, IMovieRepository
    {
        public MovieRepository(CinemaGoDbContext context) : base(context)
        {
        }

        private IQueryable<Movie> Filter(string? title, int? genreId)
        {
            var query = Context.Movies
                .Include(x => x.MovieGenres)
                    .ThenInclude(x => x.Genre)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(x => x.Title.Contains(title));
            }

            if (genreId.HasValue)
            {
                query = query.Where(x => x.MovieGenres.Any(g => g.GenreId == genreId.Value));
            }

            return query;
        }

        public async Task<IReadOnlyList<Movie>> GetFilteredAsync(
            string? title,
            int? genreId,
            int page,
            int pageSize,
            CancellationToken ct = default) =>
            await Filter(title, genreId)
                .OrderBy(x => x.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

        public Task<int> CountFilteredAsync(
            string? title,
            int? genreId,
            CancellationToken ct = default) =>
            Filter(title, genreId).CountAsync(ct);

        public Task<Movie?> GetWithGenresByIdAsync(int id, CancellationToken ct = default) =>
            Context.Movies
                .Include(x => x.MovieGenres)
                    .ThenInclude(x => x.Genre)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}
