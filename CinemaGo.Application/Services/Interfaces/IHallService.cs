using CinemaGo.Domain.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaGo.Application.Services.Interfaces
{
    public interface IHallService
    {
        Task<IReadOnlyList<HallResponse>> GetAllAsync(CancellationToken ct = default);
        Task<HallResponse> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<SeatResponse>> GetSeatsAsync(int hallId, CancellationToken ct = default);
        Task<HallResponse> CreateAsync(HallCreateRequest request, CancellationToken ct = default);
        Task<HallResponse> UpdateAsync(int id, HallUpdateRequest request, CancellationToken ct = default);
        Task<SeatResponse> UpdateSeatAsync(int hallId, int seatId, SeatUpdateRequest request, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
