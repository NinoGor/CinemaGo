using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Application.Services.Interfaces;

namespace CinemaGo.Application.Services.Implementations
{
    public sealed class ReportService(IBookingRepository bookingRepository) : IReportService
    {
        public Task<IReadOnlyList<SessionSalesDto>> GetSessionSalesAsync(DateTime? from, DateTime? to, CancellationToken ct = default) =>
            bookingRepository.GetSessionSalesAsync(from, to, ct);
    }
}