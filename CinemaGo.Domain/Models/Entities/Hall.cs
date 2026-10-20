namespace CinemaGo.Domain.Models.Entities
{
    public class Hall : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
