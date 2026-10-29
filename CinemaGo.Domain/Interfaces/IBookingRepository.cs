using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;

namespace CinemaGo.Domain.Interfaces
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<Booking?> GetWithDetailsAsync(int id, CancellationToken ct = default);

        Task<PagedResult<Booking>> GetByUserAsync(int userId, PaginationParams paging, CancellationToken ct = default);

        Task<IReadOnlyList<int>> GetBookedSeatIdsAsync(int sessionId, CancellationToken ct = default);

        Task<IReadOnlyList<Booking>> GetActiveBySessionAsync(int sessionId, CancellationToken ct = default);

        Task<bool> HasActiveBookingsAsync(int sessionId, CancellationToken ct = default);
    }
}
