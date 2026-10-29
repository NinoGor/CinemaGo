using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Infrastructure.Repositories
{
    public class MovieRepository : Repository<Movie>, IMovieRepository
    {
        public MovieRepository(CinemaGoDbContext context) : base(context) { }

        public Task<Movie?> GetWithGenresAsync(int id, CancellationToken ct = default)
            => DbSet
                .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
                .FirstOrDefaultAsync(m => m.Id == id, ct);

        public async Task<PagedResult<Movie>> SearchAsync(MovieFilter filter, CancellationToken ct = default)
        {
            var query = DbSet
                .AsNoTracking()
                .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Title))
                query = query.Where(m => m.Title.Contains(filter.Title));

            if (filter.GenreId.HasValue)
                query = query.Where(m => m.MovieGenres.Any(mg => mg.GenreId == filter.GenreId.Value));

            if (filter.MaxAgeRating.HasValue)
                query = query.Where(m => m.AgeRating <= filter.MaxAgeRating.Value);

            query = (filter.SortBy?.ToLowerInvariant(), filter.Descending) switch
            {
                ("duration", false) => query.OrderBy(m => m.DurationMinutes).ThenBy(m => m.Id),
                ("duration", true) => query.OrderByDescending(m => m.DurationMinutes).ThenBy(m => m.Id),
                (_, true) => query.OrderByDescending(m => m.Title).ThenBy(m => m.Id),
                _ => query.OrderBy(m => m.Title).ThenBy(m => m.Id)
            };

            return await query.ToPagedResultAsync(filter, ct);
        }

        public Task<bool> HasSessionsAsync(int movieId, CancellationToken ct = default)
            => Context.Sessions.AnyAsync(s => s.MovieId == movieId, ct);
    }
}
