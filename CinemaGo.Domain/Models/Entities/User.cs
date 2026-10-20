using CinemaGo.Domain.Models.Entities.Enums;

namespace CinemaGo.Domain.Models.Entities
{
    public class User : BaseEntity
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole UserRole { get; set; } = UserRole.Customer;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
