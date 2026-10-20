using CinemaGo.Domain.Interfaces.Repositories; 
using CinemaGo.Domain.Models.Entities; 
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Infrastructure.Repositories
{
    public sealed class TicketRepository : Repository<Ticket>, ITicketRepository {
        public TicketRepository(CinemaGoDbContext context) : base(context)
        {
        }

        public async Task<Ticket?> GetByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            return await Set
            .Include(t => t.Booking)
                .ThenInclude(b => b.Session)
                    .ThenInclude(s => s.Movie)
            .Include(t => t.Booking)
                .ThenInclude(b => b.Seat)
            .Include(t => t.Booking)
                .ThenInclude(b => b.User)
            .FirstOrDefaultAsync(t => t.BookingId == bookingId, cancellationToken);
        }
    }
}
