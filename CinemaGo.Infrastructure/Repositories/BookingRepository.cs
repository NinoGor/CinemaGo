using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Domain.Models.Entities.Enums;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Infrastructure.Repositories
{
    public sealed class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(CinemaGoDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<SessionSalesDto>> GetSessionSalesAsync(DateTime? from,DateTime? to,CancellationToken ct=default)
        {
            var query = from booking in Context.Bookings
                        join session in Context.Sessions on booking.SessionId equals session.Id
                        join movie in Context.Movies on session.MovieId equals movie.Id
                        join ticket in Context.Tickets on booking.Id equals ticket.BookingId
                        where booking.Status == BookingStatus.Active
                        select new { booking, session, movie, ticket };

            if (from.HasValue)
                query = query.Where(x => x.session.StartsAt >= from.Value);

            if (to.HasValue)
                query = query.Where(x => x.session.StartsAt <= to.Value);

            return await query
                .GroupBy(x => new { x.session.Id, x.movie.Title, x.session.StartsAt })
                .OrderBy(g => g.Key.StartsAt)
                .Select(g => new SessionSalesDto(
                    g.Key.Id,
                    g.Key.Title,
                    g.Key.StartsAt,
                    g.Count(),
                    g.Sum(x => x.ticket.FinalPrice)
                ))
                .ToListAsync(ct);
        }
    }
}
