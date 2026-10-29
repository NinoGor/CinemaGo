using CinemaGo.Domain.Models.DTOs;
namespace CinemaGo.Application.Services.Interfaces
{
    public interface ITicketService
    {
        Task<TicketResponse> GetByIdAsync(int ticketId, int userId, bool isAdmin, CancellationToken ct = default);
    }
}
