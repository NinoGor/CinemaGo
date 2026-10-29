using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Domain.Models.Entities.Enums;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Infrastructure.Repositories
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(CinemaGoDbContext context) : base(context) { }

        private IQueryable<Booking> WithDetails()
            => DbSet
                .Include(b => b.Session).ThenInclude(s => s.Movie)
                .Include(b => b.Session).ThenInclude(s => s.Hall)
                .Include(b => b.Seat)
                .Include(b => b.Ticket);

        public Task<Booking?> GetWithDetailsAsync(int id, CancellationToken ct = default)
            => WithDetails().FirstOrDefaultAsync(b => b.Id == id, ct);

        public async Task<PagedResult<Booking>> GetByUserAsync(
            int userId, PaginationParams paging, CancellationToken ct = default)
            => await WithDetails()
                .AsNoTracking()
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt).ThenByDescending(b => b.Id)
                .ToPagedResultAsync(paging, ct);

        public async Task<IReadOnlyList<int>> GetBookedSeatIdsAsync(int sessionId, CancellationToken ct = default)
            => await DbSet
                .AsNoTracking()
                .Where(b => b.SessionId == sessionId && b.Status == BookingStatus.Active)
                .Select(b => b.SeatId)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<Booking>> GetActiveBySessionAsync(int sessionId, CancellationToken ct = default)
            => await DbSet
                .Include(b => b.User)
                .Include(b => b.Seat)
                .Include(b => b.Ticket)
                .Where(b => b.SessionId == sessionId && b.Status == BookingStatus.Active)
                .ToListAsync(ct);

        public Task<bool> HasActiveBookingsAsync(int sessionId, CancellationToken ct = default)
            => DbSet.AnyAsync(b => b.SessionId == sessionId && b.Status == BookingStatus.Active, ct);
    }
}
