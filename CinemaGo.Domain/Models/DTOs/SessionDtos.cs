using CinemaGo.Domain.Models.Entities.Enums;
namespace CinemaGo.Domain.Models.DTOs
{
    // შექმნისთვისად (POST) და ჩანაცვლებისთვისაც (PUT)
    public record SessionRequest(
        int MovieId,
        int HallId,
        DateTime StartsAt,
        decimal StandardPrice,
        decimal VipPrice);

    public record SessionResponse(
        int Id,
        int MovieId,
        string MovieTitle,
        int HallId,
        string HallName,
        DateTime StartsAt,
        DateTime EndsAt,
        decimal StandardPrice,
        decimal VipPrice,
        SessionStatus Status);

    public class SessionFilter : PaginationParams
    {
        public int? MovieId { get; set; }
        public int? HallId { get; set; }
        public int? GenreId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        // როდესაც იქნება false და არც From გადაეცემა, მხოლოდ ჯერ არდაწყებული სესიები დაბრუნდება
        public bool IncludePast { get; set; }
        public bool IncludeCancelled { get; set; }
    }

    public record SeatStatusResponse(
        int SeatId,
        int RowNumber,
        int SeatNumber,
        bool IsVip,
        decimal Price,
        bool IsBooked);

    public record SessionSeatsResponse(
        int SessionId,
        int TotalSeats,
        int AvailableSeats,
        IReadOnlyList<SeatStatusResponse> Seats);

    public record SessionCancellationResponse(int SessionId, int CancelledBookings);

}
