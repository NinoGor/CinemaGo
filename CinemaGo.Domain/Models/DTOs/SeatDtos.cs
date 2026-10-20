using System.ComponentModel.DataAnnotations;
namespace CinemaGo.Domain.Models.DTOs
{
    public sealed record SeatDto(int Id, int HallId, int RowNumber, int SeatNumber, bool IsVip);
    public sealed class SeatWriteDto
    {
        [Range(1, int.MaxValue)] public int HallId { get; set; }
        [Range(1, 200)] public int RowNumber { get; set; }
        [Range(1, 500)] public int SeatNumber { get; set; }
        public bool IsVip { get; set; }
    }
}
