using System.ComponentModel.DataAnnotations;
using CinemaGo.Domain.Models.Entities.Enums;
namespace CinemaGo.Domain.Models.DTOs
{
    public sealed record SessionDto(int Id, int MovieId, int HallId, DateTime StartsAt, decimal StandardPrice, decimal VipPrice, SessionStatus Status);
    public sealed class SessionWriteDto
    {
        [Range(1, int.MaxValue)] public int MovieId { get; set; }
        [Range(1, int.MaxValue)] public int HallId { get; set; }
        public DateTime StartsAt { get; set; }
        [Range(typeof(decimal), "0.01", "1000000")] public decimal StandardPrice { get; set; }
        [Range(typeof(decimal), "0.01", "1000000")] public decimal VipPrice { get; set; }
    }
    public sealed record SeatAvailabilityDto(int SeatId, int RowNumber, int SeatNumber, bool IsVip, bool IsAvailable);
}
