using CinemaGo.Domain.Models.DTOs;

namespace CinemaGo.Application.Services.Interfaces
{
    public interface IReportService
    {
        Task<IReadOnlyList<MovieRevenueResponse>> GetRevenueByMovieAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
    }
}
