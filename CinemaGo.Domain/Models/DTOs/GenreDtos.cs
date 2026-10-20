using System.ComponentModel.DataAnnotations;
namespace CinemaGo.Domain.Models.DTOs
{
    public sealed record GenreDto(int Id, string Name);
    public sealed class GenreWriteDto { [Required, StringLength(80, MinimumLength = 1)] public string Name { get; set; } = string.Empty; }
}
