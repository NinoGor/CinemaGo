using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Application.Services.Interfaces;

namespace CinemaGo.Application.Services.Implementations
{
    public sealed class MovieGenreService : EntityService<MovieGenre, MovieGenreDto, MovieGenreWriteDto>, IMovieGenreService
    {
        private readonly IRepository<Movie> _movieRepository;
        private readonly IRepository<Genre> _genreRepository;
        private readonly IRepository<MovieGenre> _movieGenreRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MovieGenreService(
            IRepository<MovieGenre> movieGenreRepository,
            IRepository<Movie> movieRepository,
            IRepository<Genre> genreRepository,
            IUnitOfWork unitOfWork)
            : base(
                movieGenreRepository,
                unitOfWork,
                e => new MovieGenreDto(e.Id, e.MovieId, e.GenreId),
                d => new MovieGenre { MovieId = d.MovieId, GenreId = d.GenreId },
                (e, d) => { e.MovieId = d.MovieId; e.GenreId = d.GenreId; }
            )
        {
            _movieGenreRepository = movieGenreRepository;
            _movieRepository = movieRepository;
            _genreRepository = genreRepository;
            _unitOfWork = unitOfWork;
        }

        protected override async Task ValidateWriteAsync(MovieGenreWriteDto dto, int? currentId, CancellationToken ct)
        {
            if (await _movieRepository.GetByIdAsync(dto.MovieId, ct) is null || await _genreRepository.GetByIdAsync(dto.GenreId, ct) is null)
                throw new BusinessException("Movie or genre does not exist.", 404);

            var link = (await _movieGenreRepository.GetAllAsync(ct)).Any(x => x.MovieId == dto.MovieId && x.GenreId == dto.GenreId && x.Id != currentId);
            if (link)
                throw new BusinessException("This movie-genre relationship already exists.", 409);
        }
    }
}