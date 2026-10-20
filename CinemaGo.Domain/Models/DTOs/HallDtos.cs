using System.ComponentModel.DataAnnotations;
namespace CinemaGo.Domain.Models.DTOs
{
    public sealed record HallDto(int Id, string Name);
    public sealed class HallWriteDto { [Required, StringLength(100, MinimumLength = 1)] public string Name { get; set; } = string.Empty; }
}
