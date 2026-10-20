using CinemaGo.Domain.Models.Entities;
namespace CinemaGo.Domain.Interfaces.Repositories
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task<Ticket?> GetByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default);
    }
}
