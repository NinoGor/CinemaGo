using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Domain.Models.Entities.Enums;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Infrastructure.Repositories
{
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        public TicketRepository(CinemaGoDbContext context) : base(context) { }

        public Task<Ticket?> GetWithDetailsAsync(int id, CancellationToken ct = default)
            => DbSet
                .Include(t => t.Booking).ThenInclude(b => b.Session).ThenInclude(s => s.Movie)
                .Include(t => t.Booking).ThenInclude(b => b.Session).ThenInclude(s => s.Hall)
                .Include(t => t.Booking).ThenInclude(b => b.Seat)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

        public async Task<IReadOnlyList<MovieRevenueResponse>> GetRevenueByMovieAsync(
            DateTime? from, DateTime? to, CancellationToken ct = default)
        {
            var query = DbSet
                .AsNoTracking()
                .Where(t => t.Booking.Status == BookingStatus.Active);

            if (from.HasValue)
                query = query.Where(t => t.Booking.Session.StartsAt >= from.Value);

            if (to.HasValue)
                query = query.Where(t => t.Booking.Session.StartsAt <= to.Value);

            return await query
                .GroupBy(t => new { t.Booking.Session.MovieId, t.Booking.Session.Movie.Title })
                .OrderByDescending(g => g.Sum(t => t.FinalPrice))
                .Select(g => new MovieRevenueResponse(
                    g.Key.MovieId,
                    g.Key.Title,
                    g.Count(),
                    g.Sum(t => t.FinalPrice)))
                .ToListAsync(ct);
        }
    }
}
