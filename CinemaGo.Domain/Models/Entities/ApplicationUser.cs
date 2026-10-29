using Microsoft.AspNetCore.Identity;

namespace CinemaGo.Domain.Models.Entities
{
    // მომხმარებლები და როლები იქნება Identity-ის შესაბამის ცხრილებში
    public class ApplicationUser : IdentityUser<int>
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Booking> Bookings { get; set; } = [];
    }
    public class ApplicationRole : IdentityRole<int>
    {
        public ApplicationRole() { }

        public ApplicationRole(string roleName) : base(roleName) { }
    }
}
