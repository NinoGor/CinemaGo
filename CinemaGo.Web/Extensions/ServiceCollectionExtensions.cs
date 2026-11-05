using CinemaGo.Application.Services.Implementations;
using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Application.Settings;
using CinemaGo.Domain.Interfaces;
using CinemaGo.Infrastructure;
using CinemaGo.Infrastructure.Repositories;

namespace CinemaGo.Web.Extensions;

public static class ServiceCollectionExtensions
{
    // აფართოებს IServiceCollection ინტერფეისს, რათა ერთიანი მეთოდით დაარეგისტრიროს 
    // აპლიკაციის ყველა სერვისი, რეპოზიტორი და დამოკიდებულება DI-ისთვის
    public static IServiceCollection AddCinemaGoServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Infrastructure
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITransactionManager, TransactionManager>();

        // Repositories
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<IHallRepository, HallRepository>();
        services.AddScoped<ISeatRepository, SeatRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();

        // Application services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMovieService, MovieService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IReportService, ReportService>();

        // Supporting services
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Email:Provider = "Smtp" sends real e-mails (Gmail...), anything else only logs them.
        var emailProvider = configuration[$"{EmailSettings.SectionName}:Provider"];
        if (string.Equals(emailProvider, "Smtp", StringComparison.OrdinalIgnoreCase))
            services.AddScoped<IEmailService, SmtpEmailService>();
        else
            services.AddScoped<IEmailService, SimulatedEmailService>();

        services.AddScoped<ITicketCodeGenerator, TicketCodeGenerator>();
        services.AddScoped<IClock, SystemClock>();

        return services;
    }
}
