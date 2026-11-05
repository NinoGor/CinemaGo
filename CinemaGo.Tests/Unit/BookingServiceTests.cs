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

public class BookingServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ISessionRepository> _sessions = new();
    private readonly Mock<ISeatRepository> _seats = new();
    private readonly Mock<IBookingRepository> _bookings = new();
    private readonly Mock<IClock> _clock = new();
    private readonly Mock<ITicketCodeGenerator> _codes = new();
    private int _codeCounter;

    public BookingServiceTests()
    {
        _clock.Setup(c => c.UtcNow).Returns(TestData.Now);
        _codes.Setup(c => c.Generate()).Returns(() => $"CG-TEST{++_codeCounter}");
    }

    private BookingService CreateSut()
        => new(_sessions.Object, _seats.Object, _bookings.Object, _uow.Object, _clock.Object, _codes.Object,
            NullLogger<BookingService>.Instance);

    private void SetupSession(MovieSession session)
        => _sessions
            .Setup(r => r.GetWithDetailsAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

    private void SetupSeats(params Seat[] seats)
    {
        IReadOnlyList<Seat> list = seats;
        _seats
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
    }

    private void SetupBookedSeatIds(params int[] seatIds)
    {
        IReadOnlyList<int> list = seatIds;
        _bookings
            .Setup(r => r.GetBookedSeatIdsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
    }

    [Fact]
    public async Task Create_VipAndStandardSeats_AppliesCorrectPricesAndCreatesTickets()
    {
        var session = TestData.Session(standard: 10m, vip: 25m);
        SetupSession(session);
        SetupSeats(TestData.Seat(1, isVip: false), TestData.Seat(2, isVip: true));
        SetupBookedSeatIds();

        var result = await CreateSut().CreateAsync(7, new CreateBookingRequest(session.Id, new List<int> { 1, 2 }));

        Assert.Equal(2, result.Count);
        Assert.Equal(10m, result.Single(b => b.SeatId == 1).FinalPrice);
        Assert.Equal(25m, result.Single(b => b.SeatId == 2).FinalPrice);
        Assert.All(result, b => Assert.False(string.IsNullOrWhiteSpace(b.TicketCode)));
        Assert.Equal(result.Count, result.Select(b => b.TicketCode).Distinct().Count());

        _bookings.Verify(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once); // one transaction
    }

    [Fact]
    public async Task Create_SeatAlreadyBooked_ThrowsConflict()
    {
        var session = TestData.Session();
        SetupSession(session);
        SetupSeats(TestData.Seat(1));
        SetupBookedSeatIds(1);

        await Assert.ThrowsAsync<ConflictException>(() =>
            CreateSut().CreateAsync(7, new CreateBookingRequest(session.Id, new List<int> { 1 })));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_SeatFromAnotherHall_ThrowsBusinessRuleViolation()
    {
        var session = TestData.Session(hall: TestData.Hall(id: 1));
        SetupSession(session);
        SetupSeats(TestData.Seat(50, hallId: 2)); // seat of hall 2, session is in hall 1
        SetupBookedSeatIds();

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateSut().CreateAsync(7, new CreateBookingRequest(session.Id, new List<int> { 50 })));
    }

    [Fact]
    public async Task Create_CancelledSession_ThrowsBusinessRuleViolation()
    {
        var session = TestData.Session();
        session.Cancel();
        SetupSession(session);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            CreateSut().CreateAsync(7, new CreateBookingRequest(session.Id, new List<int> { 1 })));
    }

    [Fact]
    public async Task Cancel_BookingOfAnotherUser_ThrowsForbidden()
    {
        var session = TestData.Session();
        var booking = TestData.Booking(session, TestData.Seat(1), userId: 1);
        _bookings.Setup(r => r.GetWithDetailsAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        await Assert.ThrowsAsync<ForbiddenException>(() => CreateSut().CancelAsync(userId: 2, booking.Id));

        Assert.Equal(BookingStatus.Active, booking.Status);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Cancel_AlreadyCancelledBooking_ThrowsConflict()
    {
        var session = TestData.Session();
        var booking = TestData.Booking(session, TestData.Seat(1), userId: 1, status: BookingStatus.Cancelled);
        _bookings.Setup(r => r.GetWithDetailsAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        await Assert.ThrowsAsync<ConflictException>(() => CreateSut().CancelAsync(userId: 1, booking.Id));
    }

    [Fact]
    public async Task Cancel_OwnActiveBooking_MarksItCancelled()
    {
        var session = TestData.Session();
        var booking = TestData.Booking(session, TestData.Seat(1), userId: 1);
        _bookings.Setup(r => r.GetWithDetailsAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var result = await CreateSut().CancelAsync(userId: 1, booking.Id);

        Assert.Equal(BookingStatus.Cancelled, result.Status);
        Assert.Equal(TestData.Now, booking.CancelledAt);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
