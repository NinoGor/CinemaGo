using CinemaGo.Domain.Models.DTOs;
using FluentValidation;

namespace CinemaGo.Application.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.UserName).ValidUserName();
            RuleFor(x => x.Email).ValidEmail();
            RuleFor(x => x.Password).StrongPassword();
        }
    }

    public class RegisterAdminRequestValidator : AbstractValidator<RegisterAdminRequest>
    {
        public RegisterAdminRequestValidator()
        {
            RuleFor(x => x.UserName).ValidUserName();
            RuleFor(x => x.Email).ValidEmail();
            RuleFor(x => x.Password).StrongPassword();
            RuleFor(x => x.AdminSecret).NotEmpty();
        }
    }

    public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
    {
        public ConfirmEmailRequestValidator()
        {
            RuleFor(x => x.UserId).GreaterThan(0);
            RuleFor(x => x.Token).NotEmpty();
        }
    }

    public class ResendConfirmationRequestValidator : AbstractValidator<ResendConfirmationRequest>
    {
        public ResendConfirmationRequestValidator() => RuleFor(x => x.Email).ValidEmail();
    }

    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
