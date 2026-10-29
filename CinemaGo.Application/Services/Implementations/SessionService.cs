using System.Data;
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
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly IHallRepository _hallRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransactionManager _transactions;
        private readonly IClock _clock;
        private readonly IEmailService _email;
        private readonly ILogger<SessionService> _logger;

        public SessionService(
            ISessionRepository sessionRepository,
            IMovieRepository movieRepository,
            IHallRepository hallRepository,
            ISeatRepository seatRepository,
            IBookingRepository bookingRepository,
            IUnitOfWork unitOfWork,
            ITransactionManager transactions,
            IClock clock,
            IEmailService email,
            ILogger<SessionService> logger)
        {
            _sessionRepository = sessionRepository;
            _movieRepository = movieRepository;
            _hallRepository = hallRepository;
            _seatRepository = seatRepository;
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
            _transactions = transactions;
            _clock = clock;
            _email = email;
            _logger = logger;
        }

        public async Task<PagedResult<SessionResponse>> SearchAsync(SessionFilter filter, CancellationToken ct = default)
        {
            DateTime? defaultFrom = filter.IncludePast ? null : _clock.UtcNow;
            filter.From = filter.From?.AsUtc() ?? defaultFrom;
            filter.To = filter.To?.AsUtc();

            var page = await _sessionRepository.SearchAsync(filter, ct);
            return page.Map(s => s.ToResponse());
        }

        public async Task<SessionResponse> GetByIdAsync(int id, CancellationToken ct = default)
            => (await LoadAsync(id, ct)).ToResponse();

        public async Task<SessionSeatsResponse> GetSeatsAsync(int sessionId, CancellationToken ct = default)
        {
            var session = await LoadAsync(sessionId, ct);
            var seats = await _seatRepository.GetByHallAsync(session.HallId, ct);
            var bookedIds = (await _bookingRepository.GetBookedSeatIdsAsync(sessionId, ct)).ToHashSet();

            var seatDtos = seats
                .Select(seat => new SeatStatusResponse(
                    seat.Id,
                    seat.RowNumber,
                    seat.SeatNumber,
                    seat.IsVip,
                    session.GetPriceFor(seat),
                    bookedIds.Contains(seat.Id)))
                .ToList();

            return new SessionSeatsResponse(
                session.Id,
                seatDtos.Count,
                seatDtos.Count(s => !s.IsBooked),
                seatDtos);
        }

        public async Task<SessionResponse> CreateAsync(SessionRequest request, CancellationToken ct = default)
        {
            var movie = await _movieRepository.GetByIdAsync(request.MovieId, ct)
                        ?? throw new NotFoundException(nameof(Movie), request.MovieId);
            var hall = await _hallRepository.GetByIdAsync(request.HallId, ct)
                       ?? throw new NotFoundException(nameof(Hall), request.HallId);

            var startsAt = request.StartsAt.AsUtc();
            EnsureInFuture(startsAt);

            var session = new MovieSession
            {
                Movie = movie,
                Hall = hall,
                StandardPrice = request.StandardPrice,
                VipPrice = request.VipPrice,
                Status = SessionStatus.Scheduled
            };
            session.ScheduleAt(startsAt, movie.DurationMinutes);

            /*  ბაზა ბლოკავს დარბაზის შესაბამის დროის ინტერვალს,
                რომ სხვა პარალელურმა ტრანზაქციამ ვერც წაიკითხოს და ვერც ჩაწეროს
                (სანამ ეს პროცესი არ დასრულდება) */
            return await InSerializableTransactionAsync(async () =>
            {
                await EnsureNoOverlapAsync(hall.Id, session.StartsAt, session.EndsAt, null, ct);

                await _sessionRepository.AddAsync(session, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                _logger.LogInformation("Session {SessionId} created for movie {MovieId} in hall {HallId} at {StartsAt:u}.",
                    session.Id, movie.Id, hall.Id, session.StartsAt);
                return session.ToResponse();
            }, ct);
        }

        public async Task<SessionResponse> UpdateAsync(int id, SessionRequest request, CancellationToken ct = default)
        {
            var session = await LoadAsync(id, ct);

            if (session.IsCancelled)
                throw new ConflictException("A cancelled session cannot be modified.");

            var now = _clock.UtcNow;
            if (session.HasStarted(now))
                throw new BusinessRuleException("A session that has already started cannot be modified.");

            var movieChanged = request.MovieId != session.MovieId;
            var hallChanged = request.HallId != session.HallId;

            if ((movieChanged || hallChanged) && await _bookingRepository.HasActiveBookingsAsync(id, ct))
                throw new ConflictException("The movie or hall cannot be changed while the session has active bookings.");

            var movie = movieChanged
                ? await _movieRepository.GetByIdAsync(request.MovieId, ct) ?? throw new NotFoundException(nameof(Movie), request.MovieId)
                : session.Movie;
            var hall = hallChanged
                ? await _hallRepository.GetByIdAsync(request.HallId, ct) ?? throw new NotFoundException(nameof(Hall), request.HallId)
                : session.Hall;

            var startsAt = request.StartsAt.AsUtc();
            EnsureInFuture(startsAt);

            return await InSerializableTransactionAsync(async () =>
            {
                var endsAt = startsAt.AddMinutes(movie.DurationMinutes);
                await EnsureNoOverlapAsync(hall.Id, startsAt, endsAt, session.Id, ct);

                session.Movie = movie;
                session.MovieId = movie.Id;
                session.Hall = hall;
                session.HallId = hall.Id;
                session.ScheduleAt(startsAt, movie.DurationMinutes);
                session.StandardPrice = request.StandardPrice;
                session.VipPrice = request.VipPrice;

                await _unitOfWork.SaveChangesAsync(ct);

                _logger.LogInformation("Session {SessionId} updated.", session.Id);
                return session.ToResponse();
            }, ct);
        }

        public async Task<SessionCancellationResponse> CancelAsync(int id, CancellationToken ct = default)
        {
            var session = await LoadAsync(id, ct);
            var now = _clock.UtcNow;

            if (session.HasStarted(now) && !session.IsCancelled)
                throw new BusinessRuleException("A session that has already started cannot be cancelled.");

            session.Cancel(); // გაისვრის ConflictException-ს თუ უკვე გაუქმებულია

            var bookings = await _bookingRepository.GetActiveBySessionAsync(id, ct);
            foreach (var booking in bookings)
                booking.Cancel(now);

            // ერთი ტრანზაქციაა - სესია და მისი ჯავშნები ერთად გაუქმდება
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Session {SessionId} cancelled together with {Count} booking(s).", id, bookings.Count);

            await NotifyCustomersAsync(session, bookings, ct);
            return new SessionCancellationResponse(id, bookings.Count);
        }

        // ------------------------------------------------------------------ helpers
        private async Task<MovieSession> LoadAsync(int id, CancellationToken ct)
            => await _sessionRepository.GetWithDetailsAsync(id, ct)
               ?? throw new NotFoundException(nameof(MovieSession), id);

        private void EnsureInFuture(DateTime startsAtUtc)
        {
            if (startsAtUtc <= _clock.UtcNow)
                throw new BusinessRuleException("A session must start in the future.");
        }

        private async Task EnsureNoOverlapAsync(int hallId, DateTime start, DateTime end, int? excludeId, CancellationToken ct)
        {
            if (await _sessionRepository.HasOverlapAsync(hallId, start, end, excludeId, ct))
                throw new ConflictException("The hall already has a session that overlaps with this time range.");
        }

        private async Task<T> InSerializableTransactionAsync<T>(Func<Task<T>> work, CancellationToken ct)
        {
            await _transactions.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            try
            {
                var result = await work();
                await _transactions.CommitTransactionAsync(ct);
                return result;
            }
            catch
            {
                await _transactions.RollbackTransactionAsync(CancellationToken.None);
                throw;
            }
        }

        private async Task NotifyCustomersAsync(MovieSession session, IEnumerable<Booking> bookings, CancellationToken ct)
        {
            foreach (var booking in bookings)
            {
                if (string.IsNullOrWhiteSpace(booking.User?.Email)) continue;

                try
                {
                    await _email.SendAsync(
                        booking.User!.Email!,
                        "Your CinemaGo session was cancelled",
                        $"Hello {booking.User.UserName}, the session of '{session.Movie.Title}' on {session.StartsAt:yyyy-MM-dd HH:mm} UTC " +
                        $"was cancelled. Your booking for row {booking.Seat.RowNumber}, seat {booking.Seat.SeatNumber} " +
                        $"(ticket {booking.Ticket?.TicketCode}) is cancelled as well.",
                        ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not send cancellation e-mail for booking {BookingId}.", booking.Id);
                }
            }
        }
    }
}
