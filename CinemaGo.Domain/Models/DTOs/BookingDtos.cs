using CinemaGo.Domain.Models.Entities.Enums;

namespace CinemaGo.Domain.Models.DTOs
{
    // booking-ი არის ერთი ან რამდენიმე ადგილის ჯავშანი ერთი სესიისთვის (ატომურ ოპერაციად)
    public record CreateBookingRequest(int SessionId, List<int> SeatIds);

    public record BookingResponse(
        int Id,
        int SessionId,
        string MovieTitle,
        string HallName,
        DateTime StartsAt,
        int SeatId,
        int RowNumber,
        int SeatNumber,
        bool IsVip,
        BookingStatus Status,
        DateTime CreatedAt,
        DateTime? CancelledAt,
        int? TicketId,
        string? TicketCode,
        decimal? FinalPrice);

    public record TicketResponse(
        int Id,
        string TicketCode,
        decimal FinalPrice,
        int BookingId,
        BookingStatus BookingStatus,
        string MovieTitle,
        string HallName,
        DateTime StartsAt,
        int RowNumber,
        int SeatNumber,
        bool IsVip);

    public record MovieRevenueResponse(int MovieId, string Title, int TicketsSold, decimal Revenue);

}
