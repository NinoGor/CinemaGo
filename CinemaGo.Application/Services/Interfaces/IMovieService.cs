using CinemaGo.Domain.Models.DTOs;

namespace CinemaGo.Application.Services.Interfaces
{
    public interface IMovieService
    {
        Task<PagedResult<MovieResponse>> GetPagedAsync(MovieFilter filter, CancellationToken ct = default);
        Task<MovieResponse> GetByIdAsync(int id, CancellationToken ct = default);
        Task<MovieResponse> CreateAsync(MovieRequest request, CancellationToken ct = default);
        Task<MovieResponse> UpdateAsync(int id, MovieRequest request, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
