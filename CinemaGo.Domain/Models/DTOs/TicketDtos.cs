namespace CinemaGo.Domain.Models.DTOs
{
    public sealed record TicketDto(int Id, int BookingId, string TicketCode, decimal FinalPrice, DateTime CreatedAt);
}
