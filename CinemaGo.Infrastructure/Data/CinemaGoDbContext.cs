using CinemaGo.Domain.Models.Entities;
using CinemaGo.Domain.Models.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Infrastructure.Data
{
    public sealed class CinemaGoDbContext : DbContext
    {
        public CinemaGoDbContext(DbContextOptions<CinemaGoDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<MovieGenre> MovieGenres { get; set; }
        public DbSet<Hall> Halls { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(e =>
            {
                e.Property(x => x.UserName).HasMaxLength(60).IsRequired();
                e.Property(x => x.Email).HasMaxLength(256).IsRequired();
                e.Property(x => x.PasswordHash).IsRequired();
                e.Property(x => x.UserRole).HasConversion<int>().IsRequired();
                e.HasIndex(x => x.UserName).IsUnique();
                e.HasIndex(x => x.Email).IsUnique();
            });

            modelBuilder.Entity<Movie>(e =>
            {
                e.Property(x => x.Title).HasMaxLength(160).IsRequired();
                e.Property(x => x.Description).HasMaxLength(2000);
                e.Property(x => x.AgeRating).HasMaxLength(20).IsRequired();
                e.HasIndex(x => x.Title);
                e.ToTable(t => t.HasCheckConstraint("CK_Movies_DurationMinutes_Positive", "[DurationMinutes] > 0"));
            });

            modelBuilder.Entity<Genre>(e =>
            {
                e.Property(x => x.Name).HasMaxLength(80).IsRequired();
                e.HasIndex(x => x.Name).IsUnique();
                e.HasData(
                    new Genre { Id = 1, Name = "Action" },
                    new Genre { Id = 2, Name = "Drama" },
                    new Genre { Id = 3, Name = "Comedy" },
                    new Genre { Id = 4, Name = "Animation" }
                );
            });

            modelBuilder.Entity<MovieGenre>(e =>
            {
                e.HasKey(x => new { x.MovieId, x.GenreId });
                e.HasOne(x => x.Movie).WithMany(x => x.MovieGenres).HasForeignKey(x => x.MovieId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Genre).WithMany(x => x.MovieGenres).HasForeignKey(x => x.GenreId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Hall>(e =>
            {
                e.Property(x => x.Name).HasMaxLength(100).IsRequired();
                e.HasIndex(x => x.Name).IsUnique();
                e.HasData(
                    new Hall { Id = 1, Name = "Main Hall" },
                    new Hall { Id = 2, Name = "VIP Hall" }
                );
            });

            modelBuilder.Entity<Seat>(e =>
            {
                e.HasIndex(x => new { x.HallId, x.RowNumber, x.SeatNumber }).IsUnique();
                e.HasOne(x => x.Hall).WithMany(x => x.Seats).HasForeignKey(x => x.HallId).OnDelete(DeleteBehavior.Restrict);
                e.ToTable(t => {
                    t.HasCheckConstraint("CK_Seats_Row_Positive", "[RowNumber] > 0");
                    t.HasCheckConstraint("CK_Seats_Number_Positive", "[SeatNumber] > 0");
                });
                e.HasData(
                    new Seat { Id = 1, HallId = 1, RowNumber = 1, SeatNumber = 1, IsVip = false },
                    new Seat { Id = 2, HallId = 1, RowNumber = 1, SeatNumber = 2, IsVip = false },
                    new Seat { Id = 3, HallId = 1, RowNumber = 1, SeatNumber = 3, IsVip = true },
                    new Seat { Id = 4, HallId = 2, RowNumber = 1, SeatNumber = 1, IsVip = true },
                    new Seat { Id = 5, HallId = 2, RowNumber = 1, SeatNumber = 2, IsVip = true }
                );
            });

            modelBuilder.Entity<Session>(e =>
            {
                e.Property(x => x.StandardPrice).HasPrecision(10, 2);
                e.Property(x => x.VipPrice).HasPrecision(10, 2);
                e.HasIndex(x => new { x.HallId, x.StartsAt });
                e.HasOne(x => x.Movie).WithMany(x => x.Sessions).HasForeignKey(x => x.MovieId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Hall).WithMany(x => x.Sessions).HasForeignKey(x => x.HallId).OnDelete(DeleteBehavior.Restrict);
                e.ToTable(t => {
                    t.HasCheckConstraint("CK_Sessions_StandardPrice_NonNegative", "[StandardPrice] >= 0");
                    t.HasCheckConstraint("CK_Sessions_VipPrice_NonNegative", "[VipPrice] >= 0");
                });
            });

            modelBuilder.Entity<Booking>(e =>
            {
                e.Property(x => x.Status).HasConversion<int>();
                e.HasIndex(x => new { x.SessionId, x.SeatId }).IsUnique().HasFilter("[Status] = 0");
                e.HasIndex(x => new { x.UserId, x.CreatedAt });
                e.HasOne(x => x.User).WithMany(x => x.Bookings).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Session).WithMany(x => x.Bookings).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Seat).WithMany(x => x.Bookings).HasForeignKey(x => x.SeatId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Ticket>(e =>
            {
                e.Property(x => x.TicketCode).HasMaxLength(35).IsRequired();
                e.Property(x => x.FinalPrice).HasPrecision(10, 2);
                e.HasIndex(x => x.TicketCode).IsUnique();
                e.HasIndex(x => x.BookingId).IsUnique();
                e.HasOne(x => x.Booking).WithOne(x => x.Ticket).HasForeignKey<Ticket>(x => x.BookingId).OnDelete(DeleteBehavior.Restrict);
                e.ToTable(t => t.HasCheckConstraint("CK_Tickets_FinalPrice_NonNegative", "[FinalPrice] >= 0"));
            });
        }
    }
}
