using CinemaGo.Domain.Models.DTOs;
using FluentValidation;

namespace CinemaGo.Application.Validators
{
    public class MovieRequestValidator : AbstractValidator<MovieRequest>
    {
        public MovieRequestValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(2000);
            RuleFor(x => x.DurationMinutes).InclusiveBetween(1, 600);
            RuleFor(x => x.AgeRating).InclusiveBetween(0, 21);

            RuleFor(x => x.GenreIds)
                .NotNull()
                .NotEmpty().WithMessage("A movie needs at least one genre.");
            RuleForEach(x => x.GenreIds).GreaterThan(0);
        }
    }

    public class MovieFilterValidator : AbstractValidator<MovieFilter>
    {
        private static readonly string[] SortFields = { "title", "duration" };

        public MovieFilterValidator()
        {
            this.ApplyPagingRules();

            RuleFor(x => x.SortBy)
                .Must(s => s is null || SortFields.Contains(s.ToLowerInvariant()))
                .WithMessage("SortBy must be 'title' or 'duration'.");

            RuleFor(x => x.MaxAgeRating).InclusiveBetween(0, 21).When(x => x.MaxAgeRating.HasValue);
        }
    }
}
