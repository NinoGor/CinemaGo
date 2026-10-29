using CinemaGo.Domain.Models.DTOs;
using CinemaGo.Domain.Models.Entities;

namespace CinemaGo.Application.Mappings
{
    public static class DateTimeExtensions
    {
        // DateTime ობიექტს ყოველთვის ჰქონდეს UTC ტიპი
        public static DateTime AsUtc(this DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    // Entity -> DTO ასახვა
    public static class EntityMappings
    {
        public static GenreResponse ToResponse(this Genre genre) => new(genre.Id, genre.Name);

        public static HallResponse ToResponse(this Hall hall) => new(hall.Id, hall.Name, hall.Seats.Count);

        public static MovieResponse ToResponse(this Movie movie) => new(
            movie.Id,
            movie.Title,
            movie.Description,
            movie.DurationMinutes,
            movie.AgeRating,
            movie.MovieGenres.Select(mg => mg.Genre.ToResponse()).OrderBy(g => g.Name).ToList());

        public static SessionResponse ToResponse(this MovieSession session) => new(
            session.Id,
            session.MovieId,
            session.Movie.Title,
            session.HallId,
            session.Hall.Name,
            session.StartsAt.AsUtc(),
            session.EndsAt.AsUtc(),
            session.StandardPrice,
            session.VipPrice,
            session.Status);

        public static BookingResponse ToResponse(this Booking booking) => new(
            booking.Id,
            booking.SessionId,
            booking.Session.Movie.Title,
            booking.Session.Hall.Name,
            booking.Session.StartsAt.AsUtc(),
            booking.SeatId,
            booking.Seat.RowNumber,
            booking.Seat.SeatNumber,
            booking.Seat.IsVip,
            booking.Status,
            booking.CreatedAt.AsUtc(),
            booking.CancelledAt?.AsUtc(),
            booking.Ticket?.Id,
            booking.Ticket?.TicketCode,
            booking.Ticket?.FinalPrice);

        public static TicketResponse ToResponse(this Ticket ticket) => new(
            ticket.Id,
            ticket.TicketCode,
            ticket.FinalPrice,
            ticket.BookingId,
            ticket.Booking.Status,
            ticket.Booking.Session.Movie.Title,
            ticket.Booking.Session.Hall.Name,
            ticket.Booking.Session.StartsAt.AsUtc(),
            ticket.Booking.Seat.RowNumber,
            ticket.Booking.Seat.SeatNumber,
            ticket.Booking.Seat.IsVip);
    }
}
