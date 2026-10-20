using System;
using System.Collections.Generic;
using System.Text;

namespace CinemaGo.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
