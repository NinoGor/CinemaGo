using CinemaGo.Domain.Models.DTOs;
using FluentValidation;

namespace CinemaGo.Application.Validators
{
    // საერთო წესები DRY-ს დასაცავად
    public static class CommonRules
    {
        public static IRuleBuilderOptions<T, string> ValidUserName<T>(this IRuleBuilder<T, string> rule)
            => rule
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(50)
                .Matches("^[a-zA-Z0-9._-]+$")
                .WithMessage("'{PropertyName}' may only contain letters, digits, '.', '_' and '-'.");

        public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule)
            => rule
                .NotEmpty()
                .MaximumLength(256)
                .EmailAddress();
        public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule)
            => rule
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(100)
                .Matches("[A-Z]").WithMessage("'{PropertyName}' must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("'{PropertyName}' must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("'{PropertyName}' must contain at least one digit.");

        public static void ApplyPagingRules<T>(this AbstractValidator<T> validator) where T : PaginationParams
        {
            validator.RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            validator.RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
