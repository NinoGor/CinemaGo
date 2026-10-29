using System.Data;
using CinemaGo.Domain.Interfaces;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore; // BeginTransactionAsync-ისთვის

namespace CinemaGo.Infrastructure
{
    public class TransactionManager : ITransactionManager
    {
        private readonly CinemaGoDbContext _context;

        public TransactionManager(CinemaGoDbContext context)
        {
            _context = context;
        }

        public async Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            if (_context.Database.CurrentTransaction is not null)
                throw new InvalidOperationException("A transaction is already in progress.");

            await _context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            var transaction = _context.Database.CurrentTransaction;
            if (transaction is null) return;

            try
            {
                await transaction.CommitAsync(cancellationToken);
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            var transaction = _context.Database.CurrentTransaction;
            if (transaction is null) return;

            try
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }
    }
}
