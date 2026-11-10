using CinemaGo.Application.Mappings;
using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;
using Microsoft.Extensions.Logging;

namespace CinemaGo.Application.Services.Implementations;

public class HallService : IHallService
{
    private readonly IHallRepository _hallRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<HallService> _logger;

    public HallService(
        IHallRepository hallRepository,
        ISeatRepository seatRepository,
        IUnitOfWork unitOfWork,
        ILogger<HallService> logger)
    {
        _hallRepository = hallRepository;
        _seatRepository = seatRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IReadOnlyList<HallResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var halls = await _hallRepository.GetAllWithSeatsAsync(ct);
        return halls.Select(h => h.ToResponse()).ToList();
    }

    public async Task<HallResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var hall = await _hallRepository.GetWithSeatsAsync(id, ct)
                   ?? throw new NotFoundException(nameof(Hall), id);
        return hall.ToResponse();
    }

    public async Task<IReadOnlyList<SeatResponse>> GetSeatsAsync(int hallId, CancellationToken ct = default)
    {
        if (!await _hallRepository.ExistsAsync(hallId, ct))
            throw new NotFoundException(nameof(Hall), hallId);

        var seats = await _seatRepository.GetByHallAsync(hallId, ct);
        return seats.Select(s => s.ToResponse()).ToList();
    }

    public async Task<HallResponse> CreateAsync(HallCreateRequest request, CancellationToken ct = default)
    {
        var name = request.Name.Trim();
        await EnsureNameIsFreeAsync(name, null, ct);

        var hall = new Hall { Name = name };

        for (var row = 1; row <= request.Rows; row++)
        {
            var isVipRow = request.FirstVipRow.HasValue && row >= request.FirstVipRow.Value;
            for (var number = 1; number <= request.SeatsPerRow; number++)
                hall.Seats.Add(new Seat { RowNumber = row, SeatNumber = number, IsVip = isVipRow });
        }

        // The seats are part of the hall graph, so one SaveChanges inserts the hall and all its seats.
        await _hallRepository.AddAsync(hall, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Hall {HallId} '{Name}' created with {Seats} seats.", hall.Id, hall.Name, hall.Seats.Count);
        return hall.ToResponse();
    }

    public async Task<HallResponse> UpdateAsync(int id, HallUpdateRequest request, CancellationToken ct = default)
    {
        var hall = await _hallRepository.GetWithSeatsAsync(id, ct)
                   ?? throw new NotFoundException(nameof(Hall), id);

        var name = request.Name.Trim();
        await EnsureNameIsFreeAsync(name, id, ct);

        hall.Name = name;
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Hall {HallId} renamed to '{Name}'.", id, name);
        return hall.ToResponse();
    }

    public async Task<SeatResponse> UpdateSeatAsync(int hallId, int seatId, SeatUpdateRequest request, CancellationToken ct = default)
    {
        var seat = await _seatRepository.GetByIdAsync(seatId, ct);

        // A seat of another hall is reported as "not found" so ids of other halls are not confirmed.
        if (seat is null || !seat.BelongsToHall(hallId))
            throw new NotFoundException(nameof(Seat), seatId);

        seat.IsVip = request.IsVip;
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Seat {SeatId} of hall {HallId} VIP flag set to {IsVip}.", seatId, hallId, seat.IsVip);
        return seat.ToResponse();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var hall = await _hallRepository.GetWithSeatsAsync(id, ct)
                   ?? throw new NotFoundException(nameof(Hall), id);

        if (await _hallRepository.HasSessionsAsync(id, ct))
            throw new ConflictException("The hall has sessions and cannot be deleted.");

        // Seats -> Hall is Restrict, so the seats go first (EF orders the DELETEs by dependency).
        foreach (var seat in hall.Seats.ToList())
            _seatRepository.Delete(seat);

        _hallRepository.Delete(hall);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Hall {HallId} deleted.", id);
    }

    private async Task EnsureNameIsFreeAsync(string name, int? excludeId, CancellationToken ct)
    {
        if (await _hallRepository.NameExistsAsync(name, excludeId, ct))
            throw new ConflictException($"A hall named '{name}' already exists.");
    }
}
