using CinemaGo.Domain.Models.Entities;
namespace CinemaGo.Domain.Interfaces.Repositories
{
    public interface ISessionRepository : IRepository<Session>
    {
        Task<IReadOnlyList<Session>> GetFilteredAsync(int? movieId, DateTime? startsFrom, DateTime? startsTo, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> CountFilteredAsync(int? movieId, DateTime? startsFrom, DateTime? startsTo, CancellationToken cancellationToken = default);
    }
}
