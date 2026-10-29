using System.Data;

namespace CinemaGo.Domain.Interfaces;

// დამხმარე ინტერფეისი ტრანზაქციებისთვის, მაგალითად, ერთდროულად დაჯავშნა რომ არ მოხდეს
public interface ITransactionManager
{
    Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
