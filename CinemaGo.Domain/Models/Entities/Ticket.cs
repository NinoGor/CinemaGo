namespace CinemaGo.Domain.Models.Entities
{
    public class Ticket : BaseEntity
    {
        public int BookingId { get; set; }
        public string TicketCode { get; set; } = string.Empty;
        public decimal FinalPrice { get; set; }

        public Booking Booking { get; set; } = null!;
    }
}
