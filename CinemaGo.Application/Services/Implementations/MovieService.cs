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
        private readonly IPosterStorage _posterStorage;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MovieService> _logger;

        public const int MaxPosterBytes = 2 * 1024 * 1024;
        public MovieService(
            IMovieRepository movieRepository,
            IGenreRepository genreRepository,
            IPosterStorage posterStorage,
            IUnitOfWork unitOfWork,
            ILogger<MovieService> logger)
        {
            _movieRepository = movieRepository;
            _genreRepository = genreRepository;
            _posterStorage = posterStorage;
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
                AgeRating = request.AgeRating,
                PosterUrl = NormalizeUrl(request.PosterUrl)
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
            var oldPoster = movie.PosterUrl;
            movie.PosterUrl = NormalizeUrl(request.PosterUrl);

            // მხოლოდ ცვლილება EF-ს არ მოუწევს წაშლა და ხელახლა დამატება
            var wantedIds = genres.Select(g => g.Id).ToHashSet();

            foreach (var obsolete in movie.MovieGenres.Where(mg => !wantedIds.Contains(mg.GenreId)).ToList())
                movie.MovieGenres.Remove(obsolete);

            var existingIds = movie.MovieGenres.Select(mg => mg.GenreId).ToHashSet();
            foreach (var genre in genres.Where(g => !existingIds.Contains(g.Id)))
                movie.MovieGenres.Add(new MovieGenre { MovieId = movie.Id, GenreId = genre.Id, Genre = genre });

            await _unitOfWork.SaveChangesAsync(ct);
            await DeleteOldPosterFileAsync(oldPoster, movie.PosterUrl, movie.Id, ct);

            _logger.LogInformation("Movie {MovieId} updated.", movie.Id);
            return movie.ToResponse();
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var movie = await _movieRepository.GetByIdAsync(id, ct)
                        ?? throw new NotFoundException(nameof(Movie), id);

            if (await _movieRepository.HasSessionsAsync(id, ct))
                throw new ConflictException("The movie has sessions and cannot be deleted.");

            var poster = movie.PosterUrl;
            _movieRepository.Delete(movie);
            await _unitOfWork.SaveChangesAsync(ct);
            await DeleteOldPosterFileAsync(poster, null, id, ct);

            _logger.LogInformation("Movie {MovieId} deleted.", id);
        }

        public async Task<MovieResponse> SetPosterAsync(int id, string? posterUrl, CancellationToken ct = default)
        {
            var movie = await _movieRepository.GetWithGenresAsync(id, ct)
                        ?? throw new NotFoundException(nameof(Movie), id);

            var oldPoster = movie.PosterUrl;
            movie.PosterUrl = NormalizeUrl(posterUrl);
            await _unitOfWork.SaveChangesAsync(ct);
            await DeleteOldPosterFileAsync(oldPoster, movie.PosterUrl, id, ct);

            _logger.LogInformation("Movie {MovieId} poster link set.", id);
            return movie.ToResponse();
        }

        public async Task<MovieResponse> UploadPosterAsync(int id, Stream content, CancellationToken ct = default)
        {
            var movie = await _movieRepository.GetWithGenresAsync(id, ct)
                        ?? throw new NotFoundException(nameof(Movie), id);

            var bytes = await ReadLimitedAsync(content, ct);
            var extension = DetectImageExtension(bytes)
                            ?? throw new BusinessRuleException("The file is not a valid JPEG, PNG or WebP image.");

            var newPath = await _posterStorage.SaveAsync(bytes, extension, ct);
            var oldPoster = movie.PosterUrl;
            movie.PosterUrl = newPath;

            try
            {
                await _unitOfWork.SaveChangesAsync(ct);
            }
            catch
            {
                await _posterStorage.DeleteAsync(newPath, CancellationToken.None); // do not leave an orphan file behind
                throw;
            }

            await DeleteOldPosterFileAsync(oldPoster, newPath, id, ct);

            _logger.LogInformation("Movie {MovieId} poster uploaded ({Bytes} bytes).", id, bytes.Length);
            return movie.ToResponse();
        }

        public Task<MovieResponse> RemovePosterAsync(int id, CancellationToken ct = default)
            => SetPosterAsync(id, null, ct);

        // ამოწმებს სხვა ფილმიც ხომ არ იყენებს იმავე ფაილს და რომ ამ სთორეჯის ფაილია
        private async Task DeleteOldPosterFileAsync(string? oldPoster, string? currentPoster, int movieId, CancellationToken ct)
        {
            if (oldPoster is null || oldPoster == currentPoster || !_posterStorage.IsManaged(oldPoster))
                return;

            if (await _movieRepository.PosterUsedByOtherAsync(oldPoster, movieId, ct))
                return;

            await _posterStorage.DeleteAsync(oldPoster, ct);
        }

        private static async Task<byte[]> ReadLimitedAsync(Stream content, CancellationToken ct)
        {
            using var buffer = new MemoryStream();
            var chunk = new byte[16 * 1024];
            int read;
            while ((read = await content.ReadAsync(chunk, ct)) > 0)
            {
                buffer.Write(chunk, 0, read);
                if (buffer.Length > MaxPosterBytes)
                    throw new BusinessRuleException($"The poster must be at most {MaxPosterBytes / (1024 * 1024)} MB.");
            }

            if (buffer.Length == 0)
                throw new BusinessRuleException("The uploaded file is empty.");

            return buffer.ToArray();
        }

        // უყურებს file signature-ს, ნაცვლად ფაილის სახელისა ან Content-Type-ისა
        internal static string? DetectImageExtension(byte[] b)
        {
            if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
                return ".jpg";

            if (b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47
                && b[4] == 0x0D && b[5] == 0x0A && b[6] == 0x1A && b[7] == 0x0A)
                return ".png";

            // WebP: "RIFF" <4 byte size> "WEBP"
            if (b.Length >= 12 && b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F'
                && b[8] == 'W' && b[9] == 'E' && b[10] == 'B' && b[11] == 'P')
                return ".webp";

            return null;
        }

        private static string? NormalizeUrl(string? url)
            => string.IsNullOrWhiteSpace(url) ? null : url.Trim();
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
