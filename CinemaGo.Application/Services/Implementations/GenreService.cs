using CinemaGo.Application.Mappings;
using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;
using Microsoft.Extensions.Logging;

namespace CinemaGo.Application.Services.Implementations;

public class GenreService : IGenreService
{
    private readonly IGenreRepository _genreRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GenreService> _logger;

    public GenreService(IGenreRepository genreRepository, IUnitOfWork unitOfWork, ILogger<GenreService> logger)
    {
        _genreRepository = genreRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IReadOnlyList<GenreResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var genres = await _genreRepository.GetAllAsync(ct);
        return genres.OrderBy(g => g.Name).Select(g => g.ToResponse()).ToList();
    }

    public async Task<GenreResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var genre = await _genreRepository.GetByIdAsync(id, ct)
                    ?? throw new NotFoundException(nameof(Genre), id);
        return genre.ToResponse();
    }

    public async Task<GenreResponse> CreateAsync(GenreRequest request, CancellationToken ct = default)
    {
        var name = request.Name.Trim();
        await EnsureNameIsFreeAsync(name, null, ct);

        var genre = new Genre { Name = name };
        await _genreRepository.AddAsync(genre, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Genre {GenreId} '{Name}' created.", genre.Id, genre.Name);
        return genre.ToResponse();
    }

    public async Task<GenreResponse> UpdateAsync(int id, GenreRequest request, CancellationToken ct = default)
    {
        var genre = await _genreRepository.GetByIdAsync(id, ct)
                    ?? throw new NotFoundException(nameof(Genre), id);

        var name = request.Name.Trim();
        await EnsureNameIsFreeAsync(name, id, ct);

        genre.Name = name;
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Genre {GenreId} renamed to '{Name}'.", id, name);
        return genre.ToResponse();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var genre = await _genreRepository.GetByIdAsync(id, ct)
                    ?? throw new NotFoundException(nameof(Genre), id);

        // ჩუმი cascade delete არ გვინდა, ფილმი რომ ჟანრის გარეშე არ დარჩეს
        if (await _genreRepository.IsUsedByMoviesAsync(id, ct))
            throw new ConflictException("The genre is used by movies and cannot be deleted.");

        _genreRepository.Delete(genre);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Genre {GenreId} deleted.", id);
    }

    private async Task EnsureNameIsFreeAsync(string name, int? excludeId, CancellationToken ct)
    {
        if (await _genreRepository.NameExistsAsync(name, excludeId, ct))
            throw new ConflictException($"A genre named '{name}' already exists.");
    }
}
