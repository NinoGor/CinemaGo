using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Infrastructure.Repositories
{
    public sealed class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(CinemaGoDbContext context) : base(context)
        {
        }

        public Task<User?> GetByEmailAsync(
            string email, CancellationToken cancellationToken = default)
            => Context.Users.FirstOrDefaultAsync(
                user => user.Email == email, cancellationToken);

        public Task<User?> GetByUserNameAsync(
            string userName, CancellationToken cancellationToken = default)
            => Context.Users.FirstOrDefaultAsync(
                user => user.UserName == userName, cancellationToken);
    }
}
