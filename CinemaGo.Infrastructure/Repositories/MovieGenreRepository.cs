using CinemaGo.Domain.Interfaces.Repositories; 
using CinemaGo.Domain.Models.Entities; 
using CinemaGo.Infrastructure.Data;

namespace CinemaGo.Infrastructure.Repositories
{
    public sealed class MovieGenreRepository : Repository<MovieGenre>, IMovieGenreRepository {
        public MovieGenreRepository(CinemaGoDbContext context) : base(context)
        {
        }
    }
}
