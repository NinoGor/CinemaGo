using System.ComponentModel.DataAnnotations;
namespace CinemaGo.Domain.Models.DTOs
{
    public sealed record MovieDto(int Id, string Title, string? Description, int DurationMinutes, string AgeRating, IReadOnlyList<int> GenreIds, IReadOnlyList<string> Genres);
    public sealed class MovieWriteDto
    {
        [Required, StringLength(160, MinimumLength = 1)] public string Title { get; set; } = string.Empty;
        [StringLength(2000)] public string? Description { get; set; }
        [Range(1, 600)] public int DurationMinutes { get; set; }
        [Required, StringLength(20)] public string AgeRating { get; set; } = "PG";
        public List<int> GenreIds { get; set; } = new();
    }
}
