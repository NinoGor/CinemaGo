using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Domain.Models.Entities.Enums;
using CinemaGo.Domain.Rules;

namespace CinemaGo.Application.Services.Implementations;

public sealed class BookingService(
    IRepository<User> userRepository,
    IRepository<Session> sessionRepository,
    IRepository<Seat> seatRepository,
    IBookingRepository bookingRepository,
    ITicketRepository ticketRepository,
    IUnitOfWork unitOfWork) : IBookingService
{
    public async Task<BookingDto> CreateAsync(int userId, CreateBookingDto dto, CancellationToken ct = default)
    {
        if (await userRepository.GetByIdAsync(userId, ct) is null)
            throw new BusinessException("User was not found.", 404);

        var session = await sessionRepository.GetByIdAsync(dto.SessionId, ct);
        if (session is null)
            throw new BusinessException("Session was not found.", 404);

        if (session.Status != SessionStatus.Scheduled || session.StartsAt <= DateTime.UtcNow)
            throw new BusinessException("This session is not available for booking.", 409);

        var seat = await seatRepository.GetByIdAsync(dto.SeatId, ct);
        if (seat is null)
            throw new BusinessException("Seat was not found.", 404);

        if (seat.HallId != session.HallId)
            throw new BusinessException("The seat does not belong to the session's hall.", 400);

        if ((await bookingRepository.GetAllAsync(ct)).Any(b => b.SessionId == dto.SessionId && b.SeatId == dto.SeatId && b.Status == BookingStatus.Active))
            throw new BusinessException("That seat is already booked for this session.", 409);

        var price = BookingPriceCalculator.Calculate(seat.IsVip, session.StandardPrice, session.VipPrice);

        var booking = new Booking
        {
            UserId = userId,
            SessionId = session.Id,
            SeatId = seat.Id,
            Status = BookingStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var ticket = new Ticket
        {
            Booking = booking,
            TicketCode = TicketCodeGenerator.Generate(),
            FinalPrice = price,
            CreatedAt = DateTime.UtcNow
        };

        booking.Ticket = ticket;

        await bookingRepository.AddAsync(booking, ct);
        await ticketRepository.AddAsync(ticket, ct);

        await unitOfWork.SaveChangesAsync(ct);

        return Map(booking, ticket);
    }

    public async Task<IReadOnlyList<BookingDto>> GetMyBookingsAsync(int userId, CancellationToken ct = default)
    {
        var bookings = (await bookingRepository.GetAllAsync(ct))
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToList();

        var ids = bookings.Select(b => b.Id).ToHashSet();

        var tickets = (await ticketRepository.GetAllAsync(ct))
            .Where(t => ids.Contains(t.BookingId))
            .ToDictionary(t => t.BookingId);

        return bookings.Select(b => tickets.TryGetValue(b.Id, out var t) ? Map(b, t) : Map(b, null)).ToList();
    }

    public async Task<bool> CancelAsync(int bookingId, int userId, bool isAdmin, CancellationToken ct = default)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId, ct);
        if (booking is null) return false;

        if (!isAdmin && booking.UserId != userId)
            throw new BusinessException("You cannot cancel another user's booking.", 403);

        if (booking.Status != BookingStatus.Active)
            throw new BusinessException("This booking has already been cancelled.", 409);

        booking.Status = BookingStatus.Cancelled;
        bookingRepository.Update(booking);

        await unitOfWork.SaveChangesAsync(ct);
        return true;
    }

    private static BookingDto Map(Booking b, Ticket? t) =>
        new(b.Id, b.UserId, b.SessionId, b.SeatId, b.Status, b.CreatedAt, t?.Id, t?.TicketCode, t?.FinalPrice);
}