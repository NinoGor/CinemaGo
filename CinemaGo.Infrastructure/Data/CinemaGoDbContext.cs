using CinemaGo.Domain.Helpers;
using CinemaGo.Domain.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Infrastructure.Data;

public class CinemaGoDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public CinemaGoDbContext(DbContextOptions<CinemaGoDbContext> options) : base(options) { }

    public DbSet<Movie> Movies { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<MovieGenre> MovieGenres { get; set; }
    public DbSet<Hall> Halls { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<MovieSession> Sessions { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Ticket> Tickets { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureIdentityTables(builder);
        ConfigureCatalog(builder);
        ConfigureSessions(builder);
        ConfigureBookings(builder);
        SeedData(builder);
    }

    // ------------------------------------------------------------------ Identity - დავიხმარე AI
    private static void ConfigureIdentityTables(ModelBuilder b)
    {
        b.Entity<ApplicationUser>().ToTable("Users");
        b.Entity<ApplicationRole>().ToTable("Roles");
        b.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
        b.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        b.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        b.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        b.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

        b.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.CreatedAt).HasColumnType("datetime2");
        });
    }

    // ------------------------------------------------------------------ Movies, Genres, Halls, Seats
    private static void ConfigureCatalog(ModelBuilder b)
    {
        b.Entity<Movie>(e =>
        {
            e.ToTable("Movies", t =>
            {
                t.HasCheckConstraint("CK_Movies_Duration", "[DurationMinutes] > 0");
                t.HasCheckConstraint("CK_Movies_AgeRating", "[AgeRating] >= 0");
            });
            e.Property(m => m.Title).IsRequired().HasMaxLength(200);
            e.Property(m => m.Description).HasMaxLength(2000);
            e.HasIndex(m => m.Title);
        });

        b.Entity<Genre>(e =>
        {
            e.ToTable("Genres");
            e.Property(g => g.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(g => g.Name).IsUnique();
        });

        b.Entity<MovieGenre>(e =>
        {
            e.ToTable("MovieGenres");
            e.HasKey(mg => new { mg.MovieId, mg.GenreId });

            e.HasOne(mg => mg.Movie)
                .WithMany(m => m.MovieGenres)
                .HasForeignKey(mg => mg.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(mg => mg.Genre)
                .WithMany(g => g.MovieGenres)
                .HasForeignKey(mg => mg.GenreId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Hall>(e =>
        {
            e.ToTable("Halls");
            e.Property(h => h.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(h => h.Name).IsUnique();
        });

        b.Entity<Seat>(e =>
        {
            e.ToTable("Seats", t =>
            {
                t.HasCheckConstraint("CK_Seats_RowNumber", "[RowNumber] > 0");
                t.HasCheckConstraint("CK_Seats_SeatNumber", "[SeatNumber] > 0");
            });

            // ადგილის პოზიცია უნიკალურია დარბაზში
            e.HasIndex(s => new { s.HallId, s.RowNumber, s.SeatNumber }).IsUnique();

            e.HasOne(s => s.Hall)
                .WithMany(h => h.Seats)
                .HasForeignKey(s => s.HallId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // ------------------------------------------------------------------ Sessions
    private static void ConfigureSessions(ModelBuilder b)
    {
        b.Entity<MovieSession>(e =>
        {
            e.ToTable("Sessions", t =>
            {
                t.HasCheckConstraint("CK_Sessions_EndsAfterStart", "[EndsAt] > [StartsAt]");
                t.HasCheckConstraint("CK_Sessions_Prices", "[StandardPrice] >= 0 AND [VipPrice] >= 0");
            });

            e.Property(s => s.StartsAt).HasColumnType("datetime2");
            e.Property(s => s.EndsAt).HasColumnType("datetime2");
            e.Property(s => s.StandardPrice).HasColumnType("decimal(18,2)");
            e.Property(s => s.VipPrice).HasColumnType("decimal(18,2)");
            e.Property(s => s.Status).HasConversion<int>();

            // ინდექსი დააჩქარებს გადაფარვის შემოწმებას და ქვერიებს
            e.HasIndex(s => new { s.HallId, s.StartsAt });
            e.HasIndex(s => new { s.MovieId, s.StartsAt });

            e.HasOne(s => s.Movie)
                .WithMany(m => m.Sessions)
                .HasForeignKey(s => s.MovieId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.Hall)
                .WithMany(h => h.Sessions)
                .HasForeignKey(s => s.HallId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // ------------------------------------------------------------------ Bookings, Tickets
    private static void ConfigureBookings(ModelBuilder b)
    {
        b.Entity<Booking>(e =>
        {
            e.ToTable("Bookings");
            e.Property(x => x.Status).HasConversion<int>();
            e.Property(x => x.CreatedAt).HasColumnType("datetime2");
            e.Property(x => x.CancelledAt).HasColumnType("datetime2");

            // სესიის კონკრეტულ ადგილს მხოლოდ ერთი აქტიური ჯავშანი უნდა ჰქონდეს
            // 0 == BookingStatus.Active.
            e.HasIndex(x => new { x.SessionId, x.SeatId })
                .IsUnique()
                .HasFilter("[Status] = 0")
                .HasDatabaseName("UX_Bookings_Session_Seat_Active");

            e.HasIndex(x => x.UserId);

            e.HasOne(x => x.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Session)
                .WithMany(s => s.Bookings)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Seat)
                .WithMany(s => s.Bookings)
                .HasForeignKey(x => x.SeatId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Ticket>(e =>
        {
            e.ToTable("Tickets", t => t.HasCheckConstraint("CK_Tickets_FinalPrice", "[FinalPrice] >= 0"));
            e.Property(t => t.TicketCode).IsRequired().HasMaxLength(32);
            e.Property(t => t.FinalPrice).HasColumnType("decimal(18,2)");

            e.HasIndex(t => t.TicketCode).IsUnique();
            e.HasIndex(t => t.BookingId).IsUnique(); // ერთი-ერთთან

            e.HasOne(t => t.Booking)
                .WithOne(x => x.Ticket)
                .HasForeignKey<Ticket>(t => t.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    // ------------------------------------------------------------------ Seed data - დავიხმარე AI
    private static void SeedData(ModelBuilder b)
    {
        // ConcurrencyStamp - ყოველ ჯერზე ერთნაირი/პროგნოზირებადი რომ იყოს
        b.Entity<ApplicationRole>().HasData(
            new ApplicationRole
            {
                Id = 1,
                Name = RoleNames.Admin,
                NormalizedName = RoleNames.Admin.ToUpperInvariant(),
                ConcurrencyStamp = "5f1c7b52-0f5e-4b8e-9a3a-1d3a4c9b0001"
            },
            new ApplicationRole
            {
                Id = 2,
                Name = RoleNames.Customer,
                NormalizedName = RoleNames.Customer.ToUpperInvariant(),
                ConcurrencyStamp = "5f1c7b52-0f5e-4b8e-9a3a-1d3a4c9b0002"
            });

        b.Entity<Genre>().HasData(
            new Genre { Id = 1, Name = "Action" },
            new Genre { Id = 2, Name = "Sci-Fi" },
            new Genre { Id = 3, Name = "Drama" },
            new Genre { Id = 4, Name = "Comedy" },
            new Genre { Id = 5, Name = "Animation" },
            new Genre { Id = 6, Name = "Thriller" });

        b.Entity<Movie>().HasData(
            new Movie { Id = 1, Title = "Inception", Description = "A thief who steals secrets through dreams gets one last job.", DurationMinutes = 148, AgeRating = 13 },
            new Movie { Id = 2, Title = "The Dark Knight", Description = "Batman faces the Joker in Gotham City.", DurationMinutes = 152, AgeRating = 13 },
            new Movie { Id = 3, Title = "Spirited Away", Description = "A girl wanders into a world ruled by spirits.", DurationMinutes = 125, AgeRating = 0 },
            new Movie { Id = 4, Title = "Interstellar", Description = "Explorers travel through a wormhole to save humanity.", DurationMinutes = 169, AgeRating = 12 });

        b.Entity<MovieGenre>().HasData(
            new MovieGenre { MovieId = 1, GenreId = 1 }, new MovieGenre { MovieId = 1, GenreId = 2 },
            new MovieGenre { MovieId = 2, GenreId = 1 }, new MovieGenre { MovieId = 2, GenreId = 6 },
            new MovieGenre { MovieId = 3, GenreId = 5 },
            new MovieGenre { MovieId = 4, GenreId = 2 }, new MovieGenre { MovieId = 4, GenreId = 3 });

        b.Entity<Hall>().HasData(
            new Hall { Id = 1, Name = "Hall A" },
            new Hall { Id = 2, Name = "Hall B" });

        // დარბაზი A: 5 რიგი x 8 ადგილი (რიგები 4-5 არის VIP). დარბაზი B: 4 რიგი x 6 ადგილი (რიგი 4 არის VIP).
        var seats = new List<Seat>();
        var seatId = 1;
        void AddSeats(int hallId, int rows, int seatsPerRow, int firstVipRow)
        {
            for (var row = 1; row <= rows; row++)
                for (var number = 1; number <= seatsPerRow; number++)
                    seats.Add(new Seat
                    {
                        Id = seatId++,
                        HallId = hallId,
                        RowNumber = row,
                        SeatNumber = number,
                        IsVip = row >= firstVipRow
                    });
        }

        AddSeats(hallId: 1, rows: 5, seatsPerRow: 8, firstVipRow: 4);
        AddSeats(hallId: 2, rows: 4, seatsPerRow: 6, firstVipRow: 4);
        b.Entity<Seat>().HasData(seats);
    }
}
