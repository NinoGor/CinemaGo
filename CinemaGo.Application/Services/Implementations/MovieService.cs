using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;

namespace CinemaGo.Application.Services.Implementations
{
    public sealed class MovieService(
        IMovieRepository movieRepository,
        IRepository<Genre> genreRepository,
        IRepository<MovieGenre> movieGenreRepository,
        IRepository<Session> sessionRepository,
        IUnitOfWork unitOfWork) : IMovieService
    {
        public async Task<IReadOnlyList<MovieDto>> GetAllAsync(CancellationToken ct = default) => (await SearchAsync(null, null, 1, 1000, ct)).Items;

        public async Task<MovieDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var movie = await movieRepository.GetWithGenresByIdAsync(id, ct);
            return movie is null ? null : Map(movie);
        }

        public async Task<PagedResult<MovieDto>> SearchAsync(string? title, int? genreId, int page, int pageSize, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var rows = await movieRepository.GetFilteredAsync(title, genreId, page, pageSize, ct);
            var count = await movieRepository.CountFilteredAsync(title, genreId, ct);

            return new PagedResult<MovieDto>(rows.Select(Map).ToList(), page, pageSize, count);
        }

        public async Task<MovieDto> CreateAsync(MovieWriteDto dto, CancellationToken ct = default)
        {
            await ValidateGenreIds(dto.GenreIds, ct);
            var movie = new Movie { Title = dto.Title.Trim(), Description = dto.Description?.Trim(), DurationMinutes = dto.DurationMinutes, AgeRating = dto.AgeRating.Trim() };

            foreach (var gid in dto.GenreIds.Distinct())
                movie.MovieGenres.Add(new MovieGenre { GenreId = gid, Movie = movie });

            await movieRepository.AddAsync(movie, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return (await GetByIdAsync(movie.Id, ct))!;
        }

        public async Task<MovieDto?> UpdateAsync(int id, MovieWriteDto dto, CancellationToken ct = default)
        {
            var movie = await movieRepository.GetWithGenresByIdAsync(id, ct);
            if (movie is null) return null;

            await ValidateGenreIds(dto.GenreIds, ct);
            movie.Title = dto.Title.Trim();
            movie.Description = dto.Description?.Trim();
            movie.DurationMinutes = dto.DurationMinutes;
            movie.AgeRating = dto.AgeRating.Trim();

            var desired = dto.GenreIds.Distinct().ToHashSet();

            foreach (var old in movie.MovieGenres.Where(x => !desired.Contains(x.GenreId)).ToList())
            {
                movie.MovieGenres.Remove(old);
                movieGenreRepository.Delete(old);
            }

            var current = movie.MovieGenres.Select(x => x.GenreId).ToHashSet();

            foreach (var gid in desired.Where(x => !current.Contains(x)))
            {
                var link = new MovieGenre { MovieId = id, GenreId = gid, Movie = movie };
                movie.MovieGenres.Add(link);
                await movieGenreRepository.AddAsync(link, ct);
            }

            await unitOfWork.SaveChangesAsync(ct);
            return await GetByIdAsync(id, ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var movie = await movieRepository.GetByIdAsync(id, ct);
            if (movie is null) return false;

            if ((await sessionRepository.GetAllAsync(ct)).Any(s => s.MovieId == id))
                throw new BusinessException("A movie with scheduled or historical sessions cannot be deleted. Cancel the sessions first.", 409);

            movieRepository.Delete(movie);
            await unitOfWork.SaveChangesAsync(ct);
            return true;
        }

        private async Task ValidateGenreIds(IEnumerable<int> ids, CancellationToken ct)
        {
            var all = (await genreRepository.GetAllAsync(ct)).Select(x => x.Id).ToHashSet();
            var missing = ids.Distinct().Where(x => !all.Contains(x)).ToArray();

            if (missing.Length > 0)
                throw new BusinessException($"Unknown genre id(s): {string.Join(", ", missing)}.", 400);
        }

        private static MovieDto Map(Movie m) => new(
            m.Id,
            m.Title,
            m.Description,
            m.DurationMinutes,
            m.AgeRating,
            m.MovieGenres.Select(x => x.GenreId).Distinct().ToList(),
            m.MovieGenres.Select(x => x.Genre?.Name ?? string.Empty).Where(x => x.Length > 0).Distinct().ToList()
        );
    }
}