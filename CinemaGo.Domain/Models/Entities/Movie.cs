namespace CinemaGo.Domain.Models.Entities
{
    public class Movie : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }
        public int AgeRating { get; set; }

        public ICollection<MovieGenre> MovieGenres { get; set; } = [];
        public ICollection<MovieSession> Sessions { get; set; } = [];
    }
}
