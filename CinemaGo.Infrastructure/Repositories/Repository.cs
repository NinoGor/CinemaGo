using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Domain.Models.Entities; 
using CinemaGo.Infrastructure.Data; 
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Infrastructure.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        protected readonly CinemaGoDbContext Context;

        public Repository(CinemaGoDbContext context)
        {
            Context = context;
        }

        protected DbSet<TEntity> Set=>Context.Set<TEntity>();
        public virtual async Task<TEntity?> GetByIdAsync(int id,CancellationToken ct=default)
            => await Set.FirstOrDefaultAsync(x=>x.Id==id,ct);
        public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct=default)=>await Set.AsNoTracking().ToListAsync(ct);
        public virtual async Task AddAsync(TEntity entity,CancellationToken ct=default)
        {
            await Set.AddAsync(entity,ct);
        }
        public virtual void Update(TEntity entity)=>Set.Update(entity);
        public virtual void Delete(TEntity entity)=>Set.Remove(entity);
    }
}
