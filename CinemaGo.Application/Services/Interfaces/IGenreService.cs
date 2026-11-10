using CinemaGo.Domain.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaGo.Application.Services.Interfaces
{
    public interface IGenreService
    {
        Task<IReadOnlyList<GenreResponse>> GetAllAsync(CancellationToken ct = default);
        Task<GenreResponse> GetByIdAsync(int id, CancellationToken ct = default);
        Task<GenreResponse> CreateAsync(GenreRequest request, CancellationToken ct = default);
        Task<GenreResponse> UpdateAsync(int id, GenreRequest request, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
