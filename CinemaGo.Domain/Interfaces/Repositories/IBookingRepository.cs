using CinemaGo.Domain.Models.Entities;
namespace CinemaGo.Domain.Interfaces.Repositories
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<IReadOnlyList<CinemaGo.Domain.Models.DTOs.SessionSalesDto>> GetSessionSalesAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    }
}
