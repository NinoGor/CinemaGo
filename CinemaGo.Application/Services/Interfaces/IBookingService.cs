using CinemaGo.Domain.Models.DTOs;
namespace CinemaGo.Application.Services.Interfaces
{
    public interface IBookingService
    {
        Task<BookingDto> CreateAsync(int userId, CreateBookingDto dto, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BookingDto>> GetMyBookingsAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> CancelAsync(int bookingId, int userId, bool isAdmin, CancellationToken cancellationToken = default);
    }
}
