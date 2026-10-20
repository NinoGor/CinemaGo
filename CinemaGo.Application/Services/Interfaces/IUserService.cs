using CinemaGo.Domain.Models.DTOs;
namespace CinemaGo.Application.Services.Interfaces
{
    public interface IUserService { Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default); Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default); Task<UserDto?> UpdateAsync(int id, UserWriteDto dto, CancellationToken cancellationToken = default); }
}
