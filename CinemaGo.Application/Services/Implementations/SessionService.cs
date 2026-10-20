using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Domain.Models.Entities.Enums;
using CinemaGo.Domain.Rules;

namespace CinemaGo.Application.Services.Implementations
{
    public sealed class SessionService : EntityService<Session, SessionDto, SessionWriteDto>, ISessionService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<Hall> _hallRepository;
        private readonly IRepository<Seat> _seatRepository;
        private readonly IRepository<Booking> _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SessionService(
            ISessionRepository sessionRepository,
            IRepository<Movie> movieRepository,
            IRepository<Hall> hallRepository,
            IRepository<Seat> seatRepository,
            IRepository<Booking> bookingRepository,
            IUnitOfWork unitOfWork)
            : base(
                sessionRepository,
                unitOfWork,
                e => new SessionDto(e.Id, e.MovieId, e.HallId, e.StartsAt, e.StandardPrice, e.VipPrice, e.Status),
                d => new Session { MovieId = d.MovieId, HallId = d.HallId, StartsAt = AsUtc(d.StartsAt), StandardPrice = d.StandardPrice, VipPrice = d.VipPrice, Status = SessionStatus.Scheduled },
                (e, d) => { e.MovieId = d.MovieId; e.HallId = d.HallId; e.StartsAt = AsUtc(d.StartsAt); e.StandardPrice = d.StandardPrice; e.VipPrice = d.VipPrice; }
            )
        {
            _sessionRepository = sessionRepository;
            _movieRepository = movieRepository;
            _hallRepository = hallRepository;
            _seatRepository = seatRepository;
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<SessionDto>> SearchAsync(int? movieId, DateTime? startsFrom, DateTime? startsTo, int page, int pageSize, CancellationToken ct = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var rows = await _sessionRepository.GetFilteredAsync(movieId, startsFrom, startsTo, page, pageSize, ct);
            var count = await _sessionRepository.CountFilteredAsync(movieId, startsFrom, startsTo, ct);

            return new(rows.Select(Map).ToList(), page, pageSize, count);
        }

        public async Task<IReadOnlyList<SeatAvailabilityDto>?> GetSeatsAsync(int sessionId, CancellationToken ct = default)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId, ct);
            if (session is null) return null;

            var seats = (await _seatRepository.GetAllAsync(ct)).Where(s => s.HallId == session.HallId).OrderBy(s => s.RowNumber).ThenBy(s => s.SeatNumber).ToList();
            var taken = (await _bookingRepository.GetAllAsync(ct)).Where(b => b.SessionId == sessionId && b.Status == BookingStatus.Active).Select(b => b.SeatId).ToHashSet();

            return seats.Select(s => new SeatAvailabilityDto(s.Id, s.RowNumber, s.SeatNumber, s.IsVip, !taken.Contains(s.Id))).ToList();
        }

        protected override async Task ValidateWriteAsync(SessionWriteDto dto, int? currentId, CancellationToken ct)
        {
            var movie = await _movieRepository.GetByIdAsync(dto.MovieId, ct);
            if (movie is null) throw new BusinessException("Movie was not found.", 404);

            if (await _hallRepository.GetByIdAsync(dto.HallId, ct) is null)
                throw new BusinessException("Hall was not found.", 404);

            if (AsUtc(dto.StartsAt) <= DateTime.UtcNow)
                throw new BusinessException("A session must start in the future.", 400);

            var all = await _sessionRepository.GetAllAsync(ct);

            foreach (var other in all.Where(s => s.Id != currentId && s.HallId == dto.HallId && s.Status == SessionStatus.Scheduled))
            {
                var otherMovie = await _movieRepository.GetByIdAsync(other.MovieId, ct);
                if (otherMovie is null) continue;

                if (SessionRules.Overlaps(AsUtc(dto.StartsAt), movie.DurationMinutes, other.StartsAt, otherMovie.DurationMinutes))
                    throw new BusinessException($"The hall schedule overlaps session {other.Id}.", 409);
            }
        }

        public override async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var session = await _sessionRepository.GetByIdAsync(id, ct);
            if (session is null) return false;

            session.Status = SessionStatus.Cancelled;
            _sessionRepository.Update(session);

            foreach (var booking in (await _bookingRepository.GetAllAsync(ct)).Where(b => b.SessionId == id && b.Status == BookingStatus.Active))
            {
                booking.Status = BookingStatus.Cancelled;
                _bookingRepository.Update(booking);
            }

            await _unitOfWork.SaveChangesAsync(ct);
            return true;
        }

        private static DateTime AsUtc(DateTime value) => value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        private static SessionDto Map(Session e) => new(e.Id, e.MovieId, e.HallId, e.StartsAt, e.StandardPrice, e.VipPrice, e.Status);
    }
}