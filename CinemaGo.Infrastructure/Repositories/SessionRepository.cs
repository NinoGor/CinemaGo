using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Domain.Models.Entities; 
using CinemaGo.Infrastructure.Data; 
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Infrastructure.Repositories
{
    public sealed class SessionRepository : Repository<Session>, ISessionRepository
    {
        public SessionRepository(CinemaGoDbContext context) : base(context)
        {
        }

        private IQueryable<Session> Filter(int? movieId,DateTime? from,DateTime? to) { var q=Context.Sessions.AsQueryable(); if(movieId.HasValue)q=q.Where(x=>x.MovieId==movieId.Value); if(from.HasValue)q=q.Where(x=>x.StartsAt>=from.Value); if(to.HasValue)q=q.Where(x=>x.StartsAt<=to.Value); return q; }
        public async Task<IReadOnlyList<Session>> GetFilteredAsync(int? movieId,DateTime? from,DateTime? to,int page,int pageSize,CancellationToken ct=default)=>await Filter(movieId,from,to).OrderBy(x=>x.StartsAt).Skip((page-1)*pageSize).Take(pageSize).AsNoTracking().ToListAsync(ct);
        public Task<int> CountFilteredAsync(int? movieId,DateTime? from,DateTime? to,CancellationToken ct=default)=>Filter(movieId,from,to).CountAsync(ct);
    }
}
