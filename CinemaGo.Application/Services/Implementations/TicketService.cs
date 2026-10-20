using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Exceptions;

namespace CinemaGo.Application.Services.Implementations
{
    public sealed class TicketService(
        ITicketRepository ticketRepository,
        IBookingRepository bookingRepository) : ITicketService
    {
        public async Task<TicketDto?> GetForUserAsync(int ticketId, int userId, bool isAdmin, CancellationToken ct = default)
        {
            var ticket = await ticketRepository.GetByIdAsync(ticketId, ct);
            if (ticket is null) return null;

            var booking = await bookingRepository.GetByIdAsync(ticket.BookingId, ct);
            if (booking is null) return null;

            if (!isAdmin && booking.UserId != userId)
                throw new BusinessException("You cannot view another user's ticket.", 403);

            return new TicketDto(ticket.Id, ticket.BookingId, ticket.TicketCode, ticket.FinalPrice, ticket.CreatedAt);
        }
    }
}