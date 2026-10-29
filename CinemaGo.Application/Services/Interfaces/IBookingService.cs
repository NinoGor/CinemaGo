using CinemaGo.Domain.Models.DTOs;

namespace CinemaGo.Application.Services.Interfaces
{
    public interface IBookingService
    {
        Task<IReadOnlyList<BookingResponse>> CreateAsync(int userId, CreateBookingRequest request, CancellationToken ct = default);
        Task<BookingResponse> CancelAsync(int userId, int bookingId, CancellationToken ct = default);
        Task<BookingResponse> GetByIdAsync(int userId, int bookingId, CancellationToken ct = default);
        Task<PagedResult<BookingResponse>> GetMyBookingsAsync(int userId, PaginationParams paging, CancellationToken ct = default);
    }
}
