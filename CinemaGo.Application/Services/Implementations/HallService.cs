using CinemaGo.Domain.Interfaces;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;
using CinemaGo.Application.Services.Interfaces;

namespace CinemaGo.Application.Services.Implementations
{
    public sealed class HallService : EntityService<Hall, HallDto, HallWriteDto>, IHallService
    {
        public HallService(IRepository<Hall> hallRepository, IUnitOfWork unitOfWork)
            : base(
                hallRepository,
                unitOfWork,
                e => new HallDto(e.Id, e.Name),
                d => new Hall { Name = d.Name.Trim() },
                (e, d) => e.Name = d.Name.Trim()
            )
        {
        }
    }
}