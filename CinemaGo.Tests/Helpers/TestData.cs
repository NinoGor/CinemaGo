using CinemaGo.Domain.Models.Entities;
using CinemaGo.Domain.Models.Entities.Enums;

namespace CinemaGo.Tests.Helpers;

internal static class TestData
{
    public static readonly DateTime Now = new(2026, 10, 28, 12, 0, 0, DateTimeKind.Utc);

    public static Hall Hall(int id = 1, string name = "Hall A") => new() { Id = id, Name = name };

    public static Movie Movie(int id = 1, int duration = 120) => new()
    {
        Id = id,
        Title = $"Movie {id}",
        DurationMinutes = duration,
        AgeRating = 12
    };

    public static Seat Seat(int id, int hallId = 1, bool isVip = false) => new()
    {
        Id = id,
        HallId = hallId,
        RowNumber = 1,
        SeatNumber = id,
        IsVip = isVip
    };

    public static MovieSession Session(
        int id = 1, Hall? hall = null, Movie? movie = null, DateTime? startsAt = null,
        decimal standard = 10m, decimal vip = 25m)
    {
        hall ??= Hall();
        movie ??= Movie();

        var session = new MovieSession
        {
            Id = id,
            Hall = hall,
            HallId = hall.Id,
            Movie = movie,
            MovieId = movie.Id,
            StandardPrice = standard,
            VipPrice = vip,
            Status = SessionStatus.Scheduled
        };
        session.ScheduleAt(startsAt ?? Now.AddDays(1), movie.DurationMinutes);
        return session;
    }

    public static Booking Booking(
        MovieSession session, Seat seat, int userId = 1, int id = 1,
        BookingStatus status = BookingStatus.Active)
        => new()
        {
            Id = id,
            UserId = userId,
            User = new ApplicationUser { Id = userId, UserName = $"user{userId}", Email = $"user{userId}@test.com" },
            Session = session,
            SessionId = session.Id,
            Seat = seat,
            SeatId = seat.Id,
            Status = status,
            CreatedAt = Now,
            Ticket = new Ticket { Id = id, BookingId = id, TicketCode = $"CG-TEST{id}", FinalPrice = session.GetPriceFor(seat) }
        };
}
