using CinemaGo.Application.Mappings;
using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Domain.Models.Entities.Enums;
using Microsoft.Extensions.Logging;

namespace CinemaGo.Application.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;
        private readonly ITicketCodeGenerator _codeGenerator;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            ISessionRepository sessionRepository,
            ISeatRepository seatRepository,
            IBookingRepository bookingRepository,
            IUnitOfWork unitOfWork,
            IClock clock,
            ITicketCodeGenerator codeGenerator,
            ILogger<BookingService> logger)
        {
            _sessionRepository = sessionRepository;
            _seatRepository = seatRepository;
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
            _clock = clock;
            _codeGenerator = codeGenerator;
            _logger = logger;
        }

        public async Task<IReadOnlyList<BookingResponse>> CreateAsync(
            int userId, CreateBookingRequest request, CancellationToken ct = default)
        {
            var session = await _sessionRepository.GetWithDetailsAsync(request.SessionId, ct)
                          ?? throw new NotFoundException(nameof(MovieSession), request.SessionId);

            var now = _clock.UtcNow;

            if (session.IsCancelled)
                throw new BusinessRuleException("The session is cancelled and cannot be booked.");

            if (session.HasStarted(now))
                throw new BusinessRuleException("The session has already started.");

            var seatIds = request.SeatIds.Distinct().ToList();
            var seats = await _seatRepository.GetByIdsAsync(seatIds, ct);

            var unknown = seatIds.Except(seats.Select(s => s.Id)).ToList();
            if (unknown.Count > 0)
                throw new NotFoundException($"Seat(s) not found: {string.Join(", ", unknown)}.");

            // ბიზნეს წესი: ადგილი უნდა ეკუთვნოდეს სესიის დარბაზს
            var foreign = seats.Where(s => !s.BelongsToHall(session.HallId)).Select(s => s.Id).ToList();
            if (foreign.Count > 0)
                throw new BusinessRuleException(
                    $"Seat(s) {string.Join(", ", foreign)} do not belong to the hall '{session.Hall.Name}' of this session.");

            // ბიზნეს წესი: კონკრეტული სეესიის კონკრეტულ ადგილზე არ უნდა იყოს რამდენიმე ჯავშანი
            var alreadyBooked = (await _bookingRepository.GetBookedSeatIdsAsync(session.Id, ct)).ToHashSet();
            var taken = seatIds.Where(alreadyBooked.Contains).ToList();
            if (taken.Count > 0)
                throw new ConflictException($"Seat(s) already booked: {string.Join(", ", taken)}.");

            var bookings = new List<Booking>();
            foreach (var seat in seats)
            {
                var booking = new Booking
                {
                    UserId = userId,
                    Session = session,
                    SessionId = session.Id,
                    Seat = seat,
                    SeatId = seat.Id,
                    Status = BookingStatus.Active,
                    CreatedAt = now,
                    Ticket = new Ticket
                    {
                        TicketCode = _codeGenerator.Generate(),
                        FinalPrice = session.GetPriceFor(seat) // VIP ან standard
                    }
                };

                await _bookingRepository.AddAsync(booking, ct);
                bookings.Add(booking);
            }

            /* ყველა ადგილი და ბილეთი ინახება ერთიანი ტრანზაქციით. თუ ამასობაში სხვა 
               მოთხოვნამ უკვე დაიკავა ადგილი, უნიკალური ინდექსის ფილტრი "ჩაჭრის" მთლიან 
               შენახვას და გამომძახებელი მიიღებს 409 (კონფლიქტის აღმნიშვნელ) შეცდომას. */
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("User {UserId} booked {Count} seat(s) for session {SessionId}.",
                userId, bookings.Count, session.Id);

            return bookings.Select(b => b.ToResponse()).ToList();
        }

        public async Task<BookingResponse> CancelAsync(int userId, int bookingId, CancellationToken ct = default)
        {
            var booking = await _bookingRepository.GetWithDetailsAsync(bookingId, ct)
                          ?? throw new NotFoundException(nameof(Booking), bookingId);

            if (!booking.IsOwnedBy(userId))
            {
                _logger.LogWarning("User {UserId} tried to cancel booking {BookingId} of another user.", userId, bookingId);
                throw new ForbiddenException("You can only cancel your own bookings.");
            }

            var now = _clock.UtcNow;

            if (!booking.IsActive)
                throw new ConflictException("The booking is already cancelled.");

            if (booking.Session.HasStarted(now))
                throw new BusinessRuleException("A booking cannot be cancelled after the session has started.");

            booking.Cancel(now); 
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Booking {BookingId} cancelled by user {UserId}.", bookingId, userId);
            return booking.ToResponse();
        }

        public async Task<BookingResponse> GetByIdAsync(int userId, int bookingId, CancellationToken ct = default)
        {
            var booking = await _bookingRepository.GetWithDetailsAsync(bookingId, ct)
                          ?? throw new NotFoundException(nameof(Booking), bookingId);

            if (!booking.IsOwnedBy(userId))
                throw new ForbiddenException("You can only view your own bookings.");

            return booking.ToResponse();
        }

        public async Task<PagedResult<BookingResponse>> GetMyBookingsAsync(
            int userId, PaginationParams paging, CancellationToken ct = default)
        {
            var page = await _bookingRepository.GetByUserAsync(userId, paging, ct);
            return page.Map(b => b.ToResponse());
        }
    }
}
