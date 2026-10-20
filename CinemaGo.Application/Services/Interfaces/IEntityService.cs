namespace CinemaGo.Application.Services.Interfaces
{
    public interface IEntityService<TDto, in TWriteDto>
    {
        Task<IReadOnlyList<TDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<TDto> CreateAsync(TWriteDto dto, CancellationToken cancellationToken = default);
        Task<TDto?> UpdateAsync(int id, TWriteDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
