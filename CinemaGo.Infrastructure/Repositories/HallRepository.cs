using CinemaGo.Domain.Interfaces.Repositories; 
using CinemaGo.Domain.Models.Entities; 
using CinemaGo.Infrastructure.Data;

namespace CinemaGo.Infrastructure.Repositories
{
    public sealed class HallRepository : Repository<Hall>, IHallRepository {
        public HallRepository(CinemaGoDbContext context) : base(context)
        {
        }
    }
}
