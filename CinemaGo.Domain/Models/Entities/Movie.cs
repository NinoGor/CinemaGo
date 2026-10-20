namespace CinemaGo.Domain.Models.Entities
{
    public class Movie : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }
        public string AgeRating { get; set; } = "PG";
        public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
