using CinemaGo.Domain.Interfaces.Repositories; 
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Infrastructure.Data;

namespace CinemaGo.Infrastructure.Repositories
{
    public sealed class GenreRepository : Repository<Genre>, IGenreRepository {
        public GenreRepository(CinemaGoDbContext context) : base(context)
        {
        }
    }
}
