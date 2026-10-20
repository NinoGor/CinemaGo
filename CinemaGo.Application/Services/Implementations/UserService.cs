using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Domain.Models.Entities.Enums;

namespace CinemaGo.Application.Services.Implementations;

public sealed class UserService : IUserService
{
    private readonly IRepository<User> userRepository;
    private readonly IUnitOfWork unitOfWork;

    public UserService(
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork)
    {
        this.userRepository = userRepository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        return users
            .OrderBy(user => user.UserName)
            .Select(Map)
            .ToList();
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : Map(user);
    }

    public async Task<UserDto?> UpdateAsync(
        int id, UserWriteDto dto, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(typeof(UserRole), dto.UserRole))
        {
            throw new BusinessException("The specified user role is invalid.", 400);
        }

        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var normalizedUserName = dto.UserName.Trim();
        var normalizedEmail = dto.Email.Trim();
        var users = await userRepository.GetAllAsync(cancellationToken);

        if (users.Any(existing => existing.Id != id
            && (string.Equals(existing.UserName, normalizedUserName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(existing.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))))
        {
            throw new BusinessException("Username or email is already in use.", 409);
        }

        user.UserName = normalizedUserName;
        user.Email = normalizedEmail;
        user.UserRole = dto.UserRole;

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(user);
    }

    private static UserDto Map(User user) =>
        new(user.Id, user.UserName, user.Email, user.UserRole);
}