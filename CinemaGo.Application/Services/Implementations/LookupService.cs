using CinemaGo.Application.Mappings;
using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Models.DTOs;

namespace CinemaGo.Application.Services.Implementations
{
    public class LookupService : ILookupService
    {
        private readonly IGenreRepository _genreRepository;
        private readonly IHallRepository _hallRepository;

        public LookupService(IGenreRepository genreRepository, IHallRepository hallRepository)
        {
            _genreRepository = genreRepository;
            _hallRepository = hallRepository;
        }

        public async Task<IReadOnlyList<GenreResponse>> GetGenresAsync(CancellationToken ct = default)
        {
            var genres = await _genreRepository.GetAllAsync(ct);
            return genres.OrderBy(g => g.Name).Select(g => g.ToResponse()).ToList();
        }

        public async Task<IReadOnlyList<HallResponse>> GetHallsAsync(CancellationToken ct = default)
        {
            var halls = await _hallRepository.GetAllWithSeatsAsync(ct);
            return halls.Select(h => h.ToResponse()).ToList();
        }
    }
}
