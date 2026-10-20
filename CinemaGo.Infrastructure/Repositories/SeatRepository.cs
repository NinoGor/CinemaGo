using CinemaGo.Domain.Interfaces.Repositories; 
using CinemaGo.Domain.Models.Entities; 
using CinemaGo.Infrastructure.Data;

namespace CinemaGo.Infrastructure.Repositories
{
    public sealed class SeatRepository : Repository<Seat>, ISeatRepository {
        public SeatRepository(CinemaGoDbContext context) : base(context)
        {
        }
    }
}
