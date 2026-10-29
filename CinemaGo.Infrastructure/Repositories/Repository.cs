using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Infrastructure.Repositories;

// ჯენერიკ Base კლასი CRUD-ით
public abstract class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly CinemaGoDbContext Context;
    protected readonly DbSet<T> DbSet;

    protected Repository(CinemaGoDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await DbSet.FindAsync(new object[] { id }, ct);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await DbSet.AsNoTracking().ToListAsync(ct);

    public virtual Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => DbSet.AnyAsync(e => e.Id == id, ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
        => await DbSet.AddAsync(entity, ct);

    public virtual void Update(T entity) => DbSet.Update(entity);

    public virtual void Delete(T entity) => DbSet.Remove(entity);
}

internal static class QueryablePagingExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, PaginationParams paging, CancellationToken ct)
    {
        var total = await query.CountAsync(ct);
        var items = await query.Skip(paging.Skip).Take(paging.PageSize).ToListAsync(ct);

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = total,
            Page = paging.Page,
            PageSize = paging.PageSize
        };
    }
}
