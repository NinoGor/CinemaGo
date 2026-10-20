using CinemaGo.Domain.Models.DTOs;
namespace CinemaGo.Application.Services.Interfaces
{
    public interface IReportService
    {
        Task<IReadOnlyList<SessionSalesDto>> GetSessionSalesAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    }
}
