using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Infrastructure.Repositories
{
    public class GenreRepository : Repository<Genre>, IGenreRepository
    {
        public GenreRepository(CinemaGoDbContext context) : base(context) { }

        public async Task<IReadOnlyList<Genre>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = ids.Distinct().ToList();
            return await DbSet.Where(g => idList.Contains(g.Id)).ToListAsync(ct);
        }
        public Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
            => DbSet.AnyAsync(g => g.Name == name && (excludeId == null || g.Id != excludeId), ct);

        public Task<bool> IsUsedByMoviesAsync(int genreId, CancellationToken ct = default)
            => Context.Set<MovieGenre>().AnyAsync(mg => mg.GenreId == genreId, ct);

    }

    public class HallRepository : Repository<Hall>, IHallRepository
    {
        public HallRepository(CinemaGoDbContext context) : base(context) { }

        public async Task<IReadOnlyList<Hall>> GetAllWithSeatsAsync(CancellationToken ct = default)
            => await DbSet.AsNoTracking().Include(h => h.Seats).OrderBy(h => h.Name).ToListAsync(ct);

        public Task<Hall?> GetWithSeatsAsync(int id, CancellationToken ct = default)
            => DbSet.Include(h => h.Seats).FirstOrDefaultAsync(h => h.Id == id, ct);

        public Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
            => DbSet.AnyAsync(h => h.Name == name && (excludeId == null || h.Id != excludeId), ct);

        public Task<bool> HasSessionsAsync(int hallId, CancellationToken ct = default)
            => Context.Set<MovieSession>().AnyAsync(s => s.HallId == hallId, ct);
    }

    public class SeatRepository : Repository<Seat>, ISeatRepository
    {
        public SeatRepository(CinemaGoDbContext context) : base(context) { }

        public async Task<IReadOnlyList<Seat>> GetByHallAsync(int hallId, CancellationToken ct = default)
            => await DbSet
                .AsNoTracking()
                .Where(s => s.HallId == hallId)
                .OrderBy(s => s.RowNumber).ThenBy(s => s.SeatNumber)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<Seat>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = ids.Distinct().ToList();
            return await DbSet.Where(s => idList.Contains(s.Id)).ToListAsync(ct);
        }
    }
}
