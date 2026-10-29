using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Models.Entities.Enums;

namespace CinemaGo.Domain.Models.Entities
{
    public class Booking : BaseEntity
    {
        public int UserId { get; set; }
        public int SessionId { get; set; }
        public int SeatId { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Active;
        public DateTime CreatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public ApplicationUser User { get; set; } = null!;
        public MovieSession Session { get; set; } = null!;
        public Seat Seat { get; set; } = null!;
        public Ticket? Ticket { get; set; }

        public bool IsActive => Status == BookingStatus.Active;

        public bool IsOwnedBy(int userId) => UserId == userId;

        public void Cancel(DateTime utcNow)
        {
            if (!IsActive)
                throw new ConflictException("The booking is already cancelled.");

            Status = BookingStatus.Cancelled;
            CancelledAt = utcNow;
        }
    }
}
