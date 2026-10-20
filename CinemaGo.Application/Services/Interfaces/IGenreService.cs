using CinemaGo.Domain.Models.DTOs;
namespace CinemaGo.Application.Services.Interfaces
{
    public interface IGenreService : IEntityService<GenreDto, GenreWriteDto> { }
}
