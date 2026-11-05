using System.Data;
using CinemaGo.Application.Services.Implementations;
using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Domain.Models.Entities.Enums;
using CinemaGo.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CinemaGo.Tests.Unit;

public class SessionServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ISessionRepository> _sessions = new();
    private readonly Mock<IMovieRepository> _movies = new();
    private readonly Mock<IHallRepository> _halls = new();
    private readonly Mock<IBookingRepository> _bookings = new();
    private readonly Mock<IClock> _clock = new();
    private readonly Mock<IEmailService> _email = new();
    private readonly Mock<ISeatRepository> _seats = new();
    private readonly Mock<ITransactionManager> _tx = new();

	// დროის იმიტაცია
    public SessionServiceTests()
    {
        _clock.Setup(c => c.UtcNow).Returns(TestData.Now);
    }

	// აწყობს და აბრუნებს შესამოწმებელ სერვისს
    private SessionService CreateSut()
        => new(_sessions.Object, _movies.Object, _halls.Object, _seats.Object, _bookings.Object,
            _uow.Object, _tx.Object, _clock.Object, _email.Object, NullLogger<SessionService>.Instance);

    [Fact]
    public async Task Create_OverlappingSessionInSameHall_ThrowsConflictAndRollsBack()
    {
        var movie = TestData.Movie(duration: 120);
        var hall = TestData.Hall();
        _movies.Setup(r => r.GetByIdAsync(movie.Id, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        _halls.Setup(r => r.GetByIdAsync(hall.Id, It.IsAny<CancellationToken>())).ReturnsAsync(hall);
        _sessions
            .Setup(r => r.HasOverlapAsync(hall.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new SessionRequest(movie.Id, hall.Id, TestData.Now.AddDays(1), 10m, 20m);

        await Assert.ThrowsAsync<ConflictException>(() => CreateSut().CreateAsync(request));

        _sessions.Verify(r => r.AddAsync(It.IsAny<MovieSession>(), It.IsAny<CancellationToken>()), Times.Never);
        _tx.Verify(t => t.BeginTransactionAsync(IsolationLevel.Serializable, It.IsAny<CancellationToken>()), Times.Once);
        _tx.Verify(t => t.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_ValidRequest_EndTimeIsStartPlusMovieDuration()
    {
        var movie = TestData.Movie(duration: 150);
        var hall = TestData.Hall();
        _movies.Setup(r => r.GetByIdAsync(movie.Id, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        _halls.Setup(r => r.GetByIdAsync(hall.Id, It.IsAny<CancellationToken>())).ReturnsAsync(hall);
        _sessions
            .Setup(r => r.HasOverlapAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var startsAt = TestData.Now.AddDays(2);
        var result = await CreateSut().CreateAsync(new SessionRequest(movie.Id, hall.Id, startsAt, 10m, 20m));

        Assert.Equal(startsAt, result.StartsAt);
        Assert.Equal(startsAt.AddMinutes(150), result.EndsAt);
        Assert.Equal(SessionStatus.Scheduled, result.Status);
        _tx.Verify(t => t.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_StartInThePast_ThrowsBusinessRuleViolation()
    {
        var movie = TestData.Movie();
        var hall = TestData.Hall();
        _movies.Setup(r => r.GetByIdAsync(movie.Id, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        _halls.Setup(r => r.GetByIdAsync(hall.Id, It.IsAny<CancellationToken>())).ReturnsAsync(hall);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateSut().CreateAsync(new SessionRequest(movie.Id, hall.Id, TestData.Now.AddHours(-1), 10m, 20m)));
    }

    [Fact]
    public async Task Cancel_SessionWithBookings_CancelsAllBookingsAndSendsOneEmailEach()
    {
        var session = TestData.Session();
        var bookingA = TestData.Booking(session, TestData.Seat(1), userId: 1, id: 1);
        var bookingB = TestData.Booking(session, TestData.Seat(2), userId: 2, id: 2);
        IReadOnlyList<Booking> active = new List<Booking> { bookingA, bookingB };

        _sessions.Setup(r => r.GetWithDetailsAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _bookings.Setup(r => r.GetActiveBySessionAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(active);

        var result = await CreateSut().CancelAsync(session.Id);

        Assert.Equal(2, result.CancelledBookings);
        Assert.True(session.IsCancelled);
        Assert.Equal(BookingStatus.Cancelled, bookingA.Status);
        Assert.Equal(BookingStatus.Cancelled, bookingB.Status);

        _email.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
