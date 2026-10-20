using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Application.Services.Interfaces;

namespace CinemaGo.Application.Services.Implementations
{
    public sealed class SeatService : EntityService<Seat, SeatDto, SeatWriteDto>, ISeatService
    {
        private readonly IRepository<Hall> _hallRepository;
        private readonly IRepository<Seat> _seatRepository;

        public SeatService(
            IRepository<Seat> seatRepository,
            IRepository<Hall> hallRepository,
            IUnitOfWork unitOfWork)
            : base(
                seatRepository,
                unitOfWork,
                e => new SeatDto(e.Id, e.HallId, e.RowNumber, e.SeatNumber, e.IsVip),
                d => new Seat { HallId = d.HallId, RowNumber = d.RowNumber, SeatNumber = d.SeatNumber, IsVip = d.IsVip },
                (e, d) => { e.HallId = d.HallId; e.RowNumber = d.RowNumber; e.SeatNumber = d.SeatNumber; e.IsVip = d.IsVip; }
            )
        {
            _seatRepository = seatRepository;
            _hallRepository = hallRepository;
        }

        protected override async Task ValidateWriteAsync(SeatWriteDto dto, int? currentId, CancellationToken ct)
        {
            if (await _hallRepository.GetByIdAsync(dto.HallId, ct) is null)
                throw new BusinessException("Hall was not found.", 404);

            if ((await _seatRepository.GetAllAsync(ct)).Any(s => s.Id != currentId && s.HallId == dto.HallId && s.RowNumber == dto.RowNumber && s.SeatNumber == dto.SeatNumber))
                throw new BusinessException("That seat number already exists in this hall.", 409);
        }
    }
}