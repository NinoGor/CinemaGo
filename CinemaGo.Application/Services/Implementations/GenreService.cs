using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Application.Services.Interfaces;

namespace CinemaGo.Application.Services.Implementations
{
    public sealed class GenreService : EntityService<Genre, GenreDto, GenreWriteDto>, IGenreService
    {
        public GenreService(IRepository<Genre> genreRepository, IUnitOfWork unitOfWork)
            : base(
                genreRepository,
                unitOfWork,
                e => new GenreDto(e.Id, e.Name),
                d => new Genre { Name = d.Name.Trim() },
                (e, d) => e.Name = d.Name.Trim()
            )
        {
        }
    }
}