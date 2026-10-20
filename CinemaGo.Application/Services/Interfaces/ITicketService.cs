using CinemaGo.Domain.Models.DTOs;
namespace CinemaGo.Application.Services.Interfaces
{
    public interface ITicketService { Task<TicketDto?> GetForUserAsync(int ticketId, int userId, bool isAdmin, CancellationToken cancellationToken = default); }
}
