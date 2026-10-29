using CinemaGo.Domain.Models.Entities;

namespace CinemaGo.Domain.Interfaces
{
    // ჯენერიკ რეპოზიტორი. წაკითხვა და დამატება არის ასინქრონული
    // შენახვა იქნება IUnitOfWork-ის პასუხისმგებლობა
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);
        Task AddAsync(T entity, CancellationToken ct = default);
        void Update(T entity);
        void Delete(T entity);
    }
}
