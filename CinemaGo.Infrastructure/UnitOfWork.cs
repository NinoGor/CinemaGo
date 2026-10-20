using CinemaGo.Domain.Interfaces;
using CinemaGo.Infrastructure.Data;

namespace CinemaGo.Infrastructure.Repositories
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly CinemaGoDbContext _context;

        public UnitOfWork(CinemaGoDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}