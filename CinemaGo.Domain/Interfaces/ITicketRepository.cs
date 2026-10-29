using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;

namespace CinemaGo.Domain.Interfaces
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task<Ticket?> GetWithDetailsAsync(int id, CancellationToken ct = default);

        // გაყიდული ბილეთები და შემოსავალი თითოეული კინოსთვის (მხოლოდ აქტიური bookings)
        Task<IReadOnlyList<MovieRevenueResponse>> GetRevenueByMovieAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
    }
}
