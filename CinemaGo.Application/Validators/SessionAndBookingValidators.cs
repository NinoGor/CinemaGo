using CinemaGo.Domain.Models.DTOs;
using FluentValidation;

namespace CinemaGo.Application.Validators
{
    public class SessionRequestValidator : AbstractValidator<SessionRequest>
    {
        public SessionRequestValidator()
        {
            RuleFor(x => x.MovieId).GreaterThan(0);
            RuleFor(x => x.HallId).GreaterThan(0);
            RuleFor(x => x.StartsAt).NotEmpty();

            RuleFor(x => x.StandardPrice)
                .GreaterThanOrEqualTo(0)
                .PrecisionScale(18, 2, false);

            RuleFor(x => x.VipPrice)
                .GreaterThanOrEqualTo(0)
                .PrecisionScale(18, 2, false)
                .GreaterThanOrEqualTo(x => x.StandardPrice)
                .WithMessage("The VIP price cannot be lower than the standard price.");
        }
    }

    public class SessionFilterValidator : AbstractValidator<SessionFilter>
    {
        public SessionFilterValidator()
        {
            this.ApplyPagingRules();

            RuleFor(x => x.To)
                .Must((filter, to) => to!.Value >= filter.From!.Value)
                .When(x => x.From.HasValue && x.To.HasValue)
                .WithMessage("'To' must not be earlier than 'From'.");
        }
    }

    public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
    {
        public const int MaxSeatsPerBooking = 10;

        public CreateBookingRequestValidator()
        {
            RuleFor(x => x.SessionId).GreaterThan(0);

            RuleFor(x => x.SeatIds)
                .NotNull()
                .NotEmpty().WithMessage("Select at least one seat.")
                .Must(ids => ids.Count <= MaxSeatsPerBooking)
                .WithMessage($"You can book at most {MaxSeatsPerBooking} seats at once.");

            RuleForEach(x => x.SeatIds).GreaterThan(0);
        }
    }

    public class PaginationParamsValidator : AbstractValidator<PaginationParams>
    {
        public PaginationParamsValidator() => this.ApplyPagingRules();
    }
}
