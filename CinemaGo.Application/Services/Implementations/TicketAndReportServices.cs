using CinemaGo.Application.Mappings;
using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;

namespace CinemaGo.Application.Services.Implementations
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public async Task<TicketResponse> GetByIdAsync(int ticketId, int userId, bool isAdmin, CancellationToken ct = default)
        {
            var ticket = await _ticketRepository.GetWithDetailsAsync(ticketId, ct)
                         ?? throw new NotFoundException(nameof(Ticket), ticketId);

            // "ჩვეულებრივ" მომხმარებლებს აქვთ მხოლოდ საკუთარი ბილეთების ნახვის უფლება
            // ადმინს აქვს ნებისმიერის ნახვის უფლება
            if (!isAdmin && !ticket.Booking.IsOwnedBy(userId))
                throw new ForbiddenException("You can only view your own tickets.");

            return ticket.ToResponse();
        }
    }

    public class ReportService : IReportService
    {
        private readonly ITicketRepository _ticketRepository;

        public ReportService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public Task<IReadOnlyList<MovieRevenueResponse>> GetRevenueByMovieAsync(
            DateTime? from, DateTime? to, CancellationToken ct = default)
            => _ticketRepository.GetRevenueByMovieAsync(from?.AsUtc(), to?.AsUtc(), ct);
    }
}
