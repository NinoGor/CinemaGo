using CinemaGo.Domain.Models.DTOs;

namespace CinemaGo.Application.Services.Interfaces
{
    public interface IMovieService : IEntityService<MovieDto, MovieWriteDto>
    {
        Task<PagedResult<MovieDto>> SearchAsync(string? title, int? genreId, int page, int pageSize, CancellationToken cancellationToken = default);
    }
}
