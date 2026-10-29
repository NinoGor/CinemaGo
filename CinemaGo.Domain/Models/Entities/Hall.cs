namespace CinemaGo.Domain.Models.Entities
{
    public class Hall : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public ICollection<Seat> Seats { get; set; } = [];
        public ICollection<MovieSession> Sessions { get; set; } = [];
    }
}
