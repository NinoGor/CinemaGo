using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Models.Entities.Enums;

namespace CinemaGo.Domain.Models.Entities
{
    public class MovieSession : BaseEntity
    {
        public int MovieId { get; set; }
        public int HallId { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public decimal StandardPrice { get; set; }
        public decimal VipPrice { get; set; }
        public SessionStatus Status { get; set; } = SessionStatus.Scheduled;

        public Movie Movie { get; set; } = null!;
        public Hall Hall { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        public bool IsCancelled => Status == SessionStatus.Cancelled;

        public bool HasStarted(DateTime utcNow) => StartsAt <= utcNow;

        // ანიჭებს დასაწყისის დროს და განსაზღვრავს დასასრულის დროს ფილმის ხანგრძლივობის მიხედვით
        public void ScheduleAt(DateTime startsAtUtc, int movieDurationMinutes)
        {
            StartsAt = startsAtUtc;
            EndsAt = startsAtUtc.AddMinutes(movieDurationMinutes);
        }

        // გადაფარვის შემოწმება (თუ ეხება ინტერვალები, გადაფარვად არ ითვლება - შესაძლოა ერთი სესიის ბოლო მეორის დასაწყისი იყოს)
        public bool OverlapsWith(DateTime start, DateTime end) => StartsAt < end && EndsAt > start;
        public decimal GetPriceFor(Seat seat)
        {
            ArgumentNullException.ThrowIfNull(seat);
            return seat.IsVip ? VipPrice : StandardPrice;
        }

        public void Cancel()
        {
            if (IsCancelled)
                throw new ConflictException("The session is already cancelled.");

            Status = SessionStatus.Cancelled;
        }
    }
}
