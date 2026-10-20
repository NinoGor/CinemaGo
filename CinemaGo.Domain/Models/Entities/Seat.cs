namespace CinemaGo.Domain.Models.Entities
{
    public class Seat : BaseEntity
    {
        public int HallId { get; set; }
        public Hall Hall { get; set; } = null!;
        public int RowNumber { get; set; }
        public int SeatNumber { get; set; }
        public bool IsVip { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
