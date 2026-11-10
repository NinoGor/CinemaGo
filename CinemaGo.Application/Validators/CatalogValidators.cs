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

            RuleFor(x => x.PosterUrl)
                .MaximumLength(500)
                .Must(BeValidPosterUrl)
                .WithMessage("PosterUrl must be an absolute http(s) URL or a path starting with '/'.")
                .When(x => !string.IsNullOrWhiteSpace(x.PosterUrl));

            RuleFor(x => x.GenreIds)
                .NotNull()
                .NotEmpty().WithMessage("A movie needs at least one genre.");
            RuleForEach(x => x.GenreIds).GreaterThan(0);
        }

        // Only http(s) links or a site-relative path: blocks javascript:, data: and similar schemes.
        internal static bool BeValidPosterUrl(string? url)
        {
            url = url!.Trim();

            if (url.StartsWith('/') && !url.StartsWith("//"))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var uri)
                   && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
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

    public class GenreRequestValidator : AbstractValidator<GenreRequest>
    {
        public GenreRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        }
    }

    public class HallCreateRequestValidator : AbstractValidator<HallCreateRequest>
    {
        public HallCreateRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Rows).InclusiveBetween(1, 30);
            RuleFor(x => x.SeatsPerRow).InclusiveBetween(1, 50);

            RuleFor(x => x.FirstVipRow)
                .InclusiveBetween(1, 30)
                .Must((request, firstVipRow) => firstVipRow <= request.Rows)
                .WithMessage("The first VIP row cannot be greater than the number of rows.")
                .When(x => x.FirstVipRow.HasValue);
        }
    }

    public class HallUpdateRequestValidator : AbstractValidator<HallUpdateRequest>
    {
        public HallUpdateRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        }
    }

    public class PosterUrlRequestValidator : AbstractValidator<PosterUrlRequest>
    {
        public PosterUrlRequestValidator()
        {
            RuleFor(x => x.PosterUrl)
                .MaximumLength(500)
                .Must(MovieRequestValidator.BeValidPosterUrl)
                .WithMessage("PosterUrl must be an absolute http(s) URL or a path starting with '/'.")
                .When(x => !string.IsNullOrWhiteSpace(x.PosterUrl));
        }
    }
}
