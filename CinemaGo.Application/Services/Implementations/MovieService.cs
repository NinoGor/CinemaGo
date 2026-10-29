using CinemaGo.Application.Mappings;
using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;
using Microsoft.Extensions.Logging;

namespace CinemaGo.Application.Services.Implementations
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IGenreRepository _genreRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MovieService> _logger;

        public MovieService(
            IMovieRepository movieRepository,
            IGenreRepository genreRepository,
            IUnitOfWork unitOfWork,
            ILogger<MovieService> logger)
        {
            _movieRepository = movieRepository;
            _genreRepository = genreRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PagedResult<MovieResponse>> GetPagedAsync(MovieFilter filter, CancellationToken ct = default)
        {
            var page = await _movieRepository.SearchAsync(filter, ct);
            return page.Map(m => m.ToResponse());
        }

        public async Task<MovieResponse> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var movie = await _movieRepository.GetWithGenresAsync(id, ct)
                        ?? throw new NotFoundException(nameof(Movie), id);
            return movie.ToResponse();
        }

        public async Task<MovieResponse> CreateAsync(MovieRequest request, CancellationToken ct = default)
        {
            var genres = await ResolveGenresAsync(request.GenreIds, ct);

            var movie = new Movie
            {
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                DurationMinutes = request.DurationMinutes,
                AgeRating = request.AgeRating
            };

            foreach (var genre in genres)
                movie.MovieGenres.Add(new MovieGenre { Genre = genre });

            await _movieRepository.AddAsync(movie, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Movie {MovieId} '{Title}' created.", movie.Id, movie.Title);
            return movie.ToResponse();
        }

        public async Task<MovieResponse> UpdateAsync(int id, MovieRequest request, CancellationToken ct = default)
        {
            var movie = await _movieRepository.GetWithGenresAsync(id, ct)
                        ?? throw new NotFoundException(nameof(Movie), id);

            var genres = await ResolveGenresAsync(request.GenreIds, ct);

            movie.Title = request.Title.Trim();
            movie.Description = request.Description?.Trim();
            movie.DurationMinutes = request.DurationMinutes;
            movie.AgeRating = request.AgeRating;

            // მხოლოდ ცვლილება EF-ს არ მოუწევს წაშლა და ხელახლა დამატება
            var wantedIds = genres.Select(g => g.Id).ToHashSet();

            foreach (var obsolete in movie.MovieGenres.Where(mg => !wantedIds.Contains(mg.GenreId)).ToList())
                movie.MovieGenres.Remove(obsolete);

            var existingIds = movie.MovieGenres.Select(mg => mg.GenreId).ToHashSet();
            foreach (var genre in genres.Where(g => !existingIds.Contains(g.Id)))
                movie.MovieGenres.Add(new MovieGenre { MovieId = movie.Id, GenreId = genre.Id, Genre = genre });

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Movie {MovieId} updated.", movie.Id);
            return movie.ToResponse();
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var movie = await _movieRepository.GetByIdAsync(id, ct)
                        ?? throw new NotFoundException(nameof(Movie), id);

            if (await _movieRepository.HasSessionsAsync(id, ct))
                throw new ConflictException("The movie has sessions and cannot be deleted.");

            _movieRepository.Delete(movie);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Movie {MovieId} deleted.", id);
        }

        private async Task<IReadOnlyList<Genre>> ResolveGenresAsync(IEnumerable<int> genreIds, CancellationToken ct)
        {
            var requested = genreIds.Distinct().ToList();
            var genres = await _genreRepository.GetByIdsAsync(requested, ct);

            var missing = requested.Except(genres.Select(g => g.Id)).ToList();
            if (missing.Count > 0)
                throw new BusinessRuleException($"Unknown genre id(s): {string.Join(", ", missing)}.");

            return genres;
        }
    }
}
