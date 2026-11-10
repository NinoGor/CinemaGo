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
            e.Property(m => m.PosterUrl).HasMaxLength(500);
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
            new Movie { Id = 1, Title = "Inception", Description = "A thief who steals secrets through dreams gets one last job.", DurationMinutes = 148, AgeRating = 13, PosterUrl = "/posters/inception.jpg" },
            new Movie { Id = 2, Title = "The Dark Knight", Description = "Batman faces the Joker in Gotham City.", DurationMinutes = 152, AgeRating = 13, PosterUrl = "/posters/the-dark-knight.jpg" },
            new Movie { Id = 3, Title = "Spirited Away", Description = "A girl wanders into a world ruled by spirits.", DurationMinutes = 125, AgeRating = 0, PosterUrl = "/posters/spirited-away.jpg" },
            new Movie { Id = 4, Title = "Interstellar", Description = "Explorers travel through a wormhole to save humanity.", DurationMinutes = 169, AgeRating = 12, PosterUrl = "/posters/interstellar.jpg" });

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

        var today = DateTime.UtcNow.Date;

        b.Entity<MovieSession>().HasData(
        // --- Today's Sessions ---
        new MovieSession
        {
            Id = 1,
            MovieId = 1, // Inception
            HallId = 1,  // Hall A
            StartsAt = today.AddHours(14), // 14:00
            EndsAt = today.AddHours(16.5), // 16:28 (148 mins)
            StandardPrice = 15.00m,
            VipPrice = 25.00m,
            Status = 0 // Active/Scheduled
        },
        new MovieSession
        {
            Id = 2,
            MovieId = 2, // The Dark Knight
            HallId = 2,  // Hall B
            StartsAt = today.AddHours(17), // 17:00
            EndsAt = today.AddHours(19.6), // 19:32 (152 mins)
            StandardPrice = 18.00m,
            VipPrice = 30.00m,
            Status = 0
        },
        new MovieSession
        {
            Id = 3,
            MovieId = 4, // Interstellar
            HallId = 1,  // Hall A
            StartsAt = today.AddHours(20), // 20:00
            EndsAt = today.AddHours(22.85),// 22:49 (169 mins)
            StandardPrice = 20.00m,
            VipPrice = 35.00m,
            Status = 0
        },

        // --- Tomorrow's Sessions ---
        new MovieSession
        {
            Id = 4,
            MovieId = 3, // Spirited Away
            HallId = 2,  // Hall B
            StartsAt = today.AddDays(1).AddHours(12), // Tomorrow at 12:00
            EndsAt = today.AddDays(1).AddHours(14.1), // 14:05 (125 mins)
            StandardPrice = 12.00m,
            VipPrice = 20.00m,
            Status = 0
        },
        new MovieSession
        {
            Id = 5,
            MovieId = 1, // Inception
            HallId = 2,  // Hall B
            StartsAt = today.AddDays(1).AddHours(15), // Tomorrow at 15:00
            EndsAt = today.AddDays(1).AddHours(17.5), // 17:28
            StandardPrice = 15.00m,
            VipPrice = 25.00m,
            Status = 0
        },
        new MovieSession
        {
            Id = 6,
            MovieId = 2, // The Dark Knight
            HallId = 1,  // Hall A
            StartsAt = today.AddDays(1).AddHours(18), // Tomorrow at 18:00
            EndsAt = today.AddDays(1).AddHours(20.6), // 20:32
            StandardPrice = 18.00m,
            VipPrice = 30.00m,
            Status = 0
        },

        // --- Day After Tomorrow's Sessions ---
        new MovieSession
        {
            Id = 7,
            MovieId = 4, // Interstellar
            HallId = 1,  // Hall A
            StartsAt = today.AddDays(2).AddHours(16), // In 2 days at 16:00
            EndsAt = today.AddDays(2).AddHours(18.85), // 18:49
            StandardPrice = 20.00m,
            VipPrice = 35.00m,
            Status = 0
        },
        new MovieSession
        {
            Id = 8,
            MovieId = 3, // Spirited Away
            HallId = 2,  // Hall B
            StartsAt = today.AddDays(2).AddHours(19), // In 2 days at 19:00
            EndsAt = today.AddDays(2).AddHours(21.1),  // 21:05
            StandardPrice = 14.00m,
            VipPrice = 22.00m,
            Status = 0
        }
    );

    }
}
