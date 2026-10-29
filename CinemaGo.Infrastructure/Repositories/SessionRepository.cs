using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Domain.Models.Entities.Enums;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Infrastructure.Repositories
{
    public class SessionRepository : Repository<MovieSession>, ISessionRepository
    {
        public SessionRepository(CinemaGoDbContext context) : base(context) { }

        public Task<MovieSession?> GetWithDetailsAsync(int id, CancellationToken ct = default)
            => DbSet
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.Id == id, ct);

        public async Task<PagedResult<MovieSession>> SearchAsync(SessionFilter filter, CancellationToken ct = default)
        {
            var query = DbSet
                .AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .AsQueryable();

            if (!filter.IncludeCancelled)
                query = query.Where(s => s.Status == SessionStatus.Scheduled);

            if (filter.MovieId.HasValue)
                query = query.Where(s => s.MovieId == filter.MovieId.Value);

            if (filter.HallId.HasValue)
                query = query.Where(s => s.HallId == filter.HallId.Value);

            if (filter.GenreId.HasValue)
                query = query.Where(s => s.Movie.MovieGenres.Any(mg => mg.GenreId == filter.GenreId.Value));

            if (filter.From.HasValue)
                query = query.Where(s => s.StartsAt >= filter.From.Value);

            if (filter.To.HasValue)
                query = query.Where(s => s.StartsAt <= filter.To.Value);

            query = query.OrderBy(s => s.StartsAt).ThenBy(s => s.Id);

            return await query.ToPagedResultAsync(filter, ct);
        }

        public Task<bool> HasOverlapAsync(
            int hallId, DateTime start, DateTime end, int? excludeSessionId, CancellationToken ct = default)
            => DbSet.AnyAsync(s =>
                s.HallId == hallId &&
                s.Status == SessionStatus.Scheduled &&
                (excludeSessionId == null || s.Id != excludeSessionId) &&
                s.StartsAt < end &&
                s.EndsAt > start, ct);
    }
}
