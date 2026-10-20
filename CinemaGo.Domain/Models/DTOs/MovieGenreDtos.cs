using System.ComponentModel.DataAnnotations;
namespace CinemaGo.Domain.Models.DTOs
{
    public sealed record MovieGenreDto(int Id, int MovieId, int GenreId);
    public sealed class MovieGenreWriteDto { [Range(1, int.MaxValue)] public int MovieId { get; set; } [Range(1, int.MaxValue)] public int GenreId { get; set; } }
}
