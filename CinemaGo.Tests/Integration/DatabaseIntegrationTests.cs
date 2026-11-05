using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Helpers;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Domain.Models.Entities.Enums;
using CinemaGo.Infrastructure;
using CinemaGo.Infrastructure.Data;
using CinemaGo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Tests.Integration;

[Trait("Category", "Integration")]
public class DatabaseIntegrationTests : IClassFixture<SqlServerFixture>
{
    private readonly SqlServerFixture _fixture;

    public DatabaseIntegrationTests(SqlServerFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task SeedData_IsPresent()
    {
        await using var db = _fixture.CreateContext();

        Assert.Equal(2, await db.Halls.CountAsync());
        Assert.Equal(5 * 8 + 4 * 6, await db.Seats.CountAsync());
        Assert.True(await db.Movies.CountAsync() >= 4);
        Assert.True(await db.Genres.AnyAsync());
        Assert.Contains(RoleNames.Admin, await db.Roles.Select(r => r.Name!).ToListAsync());
        Assert.Contains(RoleNames.Customer, await db.Roles.Select(r => r.Name!).ToListAsync());
    }

    [Fact]
    public async Task DoubleBooking_SameSeatSameSession_ThrowsConflict()
    {
        var (sessionId, seatId, userId) = await ArrangeAsync();

        await BookAsync(userId, sessionId, seatId); // პირველი ჯავშანი წარმატებულია

        // მეორე აქტიური ჯავშანი იმავე ადგილზე უარყოფილია unique ინდექსის მიერ
        await Assert.ThrowsAsync<ConflictException>(() => BookAsync(userId, sessionId, seatId));
    }

    [Fact]
    public async Task Booking_AfterCancellation_SeatCanBeBookedAgain()
    {
        var (sessionId, seatId, userId) = await ArrangeAsync();

        var firstBookingId = await BookAsync(userId, sessionId, seatId);

        await using (var db = _fixture.CreateContext())
        {
            var bookings = new BookingRepository(db);
            var unitOfWork = new UnitOfWork(db);

            var booking = await bookings.GetByIdAsync(firstBookingId);
            booking!.Cancel(DateTime.UtcNow);
            await unitOfWork.SaveChangesAsync();
        }

        //  აქ არ უნდა მოხდეს throw, რადგან unique index მხოლოდ აქტიურ ჯავშნებს ეხება
        var secondBookingId = await BookAsync(userId, sessionId, seatId);
        Assert.NotEqual(firstBookingId, secondBookingId);
    }

    [Fact]
    public async Task OverlapQuery_FindsOverlappingSessionsInSameHallOnly()
    {
        await using var db = _fixture.CreateContext();
        var sessions = new SessionRepository(db);
        var unitOfWork = new UnitOfWork(db);

        var start = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var session = new MovieSession
        {
            MovieId = 1, HallId = 1, StandardPrice = 10, VipPrice = 20,
            StartsAt = start, EndsAt = start.AddMinutes(120)
        };
        await sessions.AddAsync(session);
        await unitOfWork.SaveChangesAsync();

        Assert.True(await sessions.HasOverlapAsync(1, start.AddMinutes(60), start.AddMinutes(180), null));
        Assert.False(await sessions.HasOverlapAsync(2, start.AddMinutes(60), start.AddMinutes(180), null)); // სხვა დარბაზი
        Assert.False(await sessions.HasOverlapAsync(1, start.AddMinutes(120), start.AddMinutes(240), null)); // ინტერვალები "ეხება"
        // სესიის განახლებისას, სისტემამ რომ არ იფიქროს რომ ეს დრო უკვე დაკავებულია
		Assert.False(await sessions.HasOverlapAsync(1, start.AddMinutes(60), start.AddMinutes(180), session.Id)); // გამოირიცხება თვითონ ეს სესია
		
    }

    // ------------------------------------------------------------------ helpers
    private static int _counter;

    /// <summary>Creates a user and a future session in hall 1 and returns (sessionId, seatId, userId).</summary>
    private async Task<(int SessionId, int SeatId, int UserId)> ArrangeAsync()
    {
        var n = Interlocked.Increment(ref _counter);
        await using var db = _fixture.CreateContext();

        var user = new ApplicationUser
        {
            UserName = $"it-user-{n}-{Guid.NewGuid():N}",
            NormalizedUserName = $"IT-USER-{n}-{Guid.NewGuid():N}".ToUpperInvariant(),
            Email = $"it{n}@test.com",
            NormalizedEmail = $"IT{n}@TEST.COM",
            SecurityStamp = Guid.NewGuid().ToString()
        };
        db.Users.Add(user);

        var start = new DateTime(2031, 1, 1, 10, 0, 0, DateTimeKind.Utc).AddDays(n);
        var session = new MovieSession
        {
            MovieId = 1, HallId = 1, StandardPrice = 10, VipPrice = 20,
            StartsAt = start, EndsAt = start.AddMinutes(148)
        };
        db.Sessions.Add(session);

        await db.SaveChangesAsync();

        var seatId = await db.Seats.Where(s => s.HallId == 1).Select(s => s.Id).FirstAsync();
        return (session.Id, seatId, user.Id);
    }

    private async Task<int> BookAsync(int userId, int sessionId, int seatId)
    {
        await using var db = _fixture.CreateContext();
        var bookings = new BookingRepository(db);
        var unitOfWork = new UnitOfWork(db);

        var booking = new Booking
        {
            UserId = userId,
            SessionId = sessionId,
            SeatId = seatId,
            Status = BookingStatus.Active,
            CreatedAt = DateTime.UtcNow,
            Ticket = new Ticket { TicketCode = "CG-" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(), FinalPrice = 10 }
        };

        await bookings.AddAsync(booking);
        await unitOfWork.SaveChangesAsync();
        return booking.Id;
    }
}
