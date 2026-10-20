using System.ComponentModel.DataAnnotations;
using CinemaGo.Domain.Models.Entities.Enums;
namespace CinemaGo.Domain.Models.DTOs
{
    public sealed record BookingDto(int Id, int UserId, int SessionId, int SeatId, BookingStatus Status, DateTime CreatedAt, int? TicketId, string? TicketCode, decimal? FinalPrice);
    public sealed class CreateBookingDto
    {
        [Range(1, int.MaxValue)] public int SessionId { get; set; }
        [Range(1, int.MaxValue)] public int SeatId { get; set; }
    }
}
