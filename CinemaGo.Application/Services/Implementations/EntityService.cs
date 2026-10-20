using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Application.Services.Interfaces;
namespace CinemaGo.Application.Services.Implementations
{
    public abstract class EntityService<TEntity, TDto, TWriteDto> : IEntityService<TDto, TWriteDto>
        where TEntity : BaseEntity
    {
        protected readonly IRepository<TEntity> Repository;
        protected readonly IUnitOfWork UnitOfWork;
        private readonly Func<TEntity, TDto> _toDto;
        private readonly Func<TWriteDto, TEntity> _create;
        private readonly Action<TEntity, TWriteDto> _update;

        protected EntityService(IRepository<TEntity> repository, IUnitOfWork unitOfWork,
            Func<TEntity, TDto> toDto, Func<TWriteDto, TEntity> create, Action<TEntity, TWriteDto> update)
        { Repository = repository; UnitOfWork = unitOfWork; _toDto = toDto; _create = create; _update = update; }

        public virtual async Task<IReadOnlyList<TDto>> GetAllAsync(CancellationToken cancellationToken = default)
        { var entities = await Repository.GetAllAsync(cancellationToken); return entities.Select(_toDto).ToList(); }

        public virtual async Task<TDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        { var entity = await Repository.GetByIdAsync(id, cancellationToken); return entity is null ? default : _toDto(entity); }

        public virtual async Task<TDto> CreateAsync(TWriteDto dto, CancellationToken cancellationToken = default)
        { await ValidateWriteAsync(dto, null, cancellationToken); var entity = _create(dto); await Repository.AddAsync(entity, cancellationToken); await UnitOfWork.SaveChangesAsync(cancellationToken); return _toDto(entity); }

        public virtual async Task<TDto?> UpdateAsync(int id, TWriteDto dto, CancellationToken cancellationToken = default)
        { var entity = await Repository.GetByIdAsync(id, cancellationToken); if (entity is null) return default; await ValidateWriteAsync(dto, id, cancellationToken); _update(entity, dto); Repository.Update(entity); await UnitOfWork.SaveChangesAsync(cancellationToken); return _toDto(entity); }

        public virtual async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        { var entity = await Repository.GetByIdAsync(id, cancellationToken); if (entity is null) return false; Repository.Delete(entity); await UnitOfWork.SaveChangesAsync(cancellationToken); return true; }

        protected virtual Task ValidateWriteAsync(TWriteDto dto, int? currentId, CancellationToken cancellationToken) => Task.CompletedTask;
        protected static BusinessException NotFound(string name, int id) => new($"{name} with id {id} was not found.", 404);
    }
}
