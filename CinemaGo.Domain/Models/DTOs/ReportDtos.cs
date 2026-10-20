namespace CinemaGo.Domain.Models.DTOs
{
    public sealed record SessionSalesDto(int SessionId, string MovieTitle, DateTime StartsAt, int ActiveBookingCount, decimal Revenue);
}
