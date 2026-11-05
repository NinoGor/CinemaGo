using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CinemaGo.Web.Filters;

// უშვებს FluentValidation ვალიდატორს (თუ ის რეგისტრირებულია) თითოეული არგუმენტისთვის
// და ვალიდაციის ჩავარდნის შემთხვევაში აჩერებს მოთხოვნას 400 ValidationProblemDetails პასუხით.

public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // ინახავს ვალიდაციის დროს აღმოჩენილ ყველა შეცდომას
        var failures = new List<ValidationFailure>();

        // გადის კონტროლერის მეთოდის ყველა შემავალ არგუმენტზე
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            // დინამიურად ადგენს და ეძებს შესაბამის IValidator ტიპს თითოეული არგუმენტისთვის
            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (_serviceProvider.GetService(validatorType) is not IValidator validator) continue;

            // უშვებს ვალიდაციის პროცესს ასინქრონულად
            var result = await validator.ValidateAsync(
                new ValidationContext<object>(argument),
                context.HttpContext.RequestAborted);

            // აღმოჩენილ შეცდომებს ამატებს საერთო სიაში
            failures.AddRange(result.Errors);
        }

        // თუ ვალიდაციისას დაფიქსირდა შეცდომები
        if (failures.Count > 0)
        {
            // აჯგუფებს შეცდომებს ველების (PropertyName) მიხედვით
            var errors = failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).Distinct().ToArray());

            // აჩერებს მოთხოვნის შემდგომ შესრულებას და აბრუნებს 400 Bad Request პასუხს შეცდომების დეტალებით
            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Instance = context.HttpContext.Request.Path
            });
            return;
        }

        // თუ შეცდომები არ არის, მოთხოვნა აგრძელებს გზას კონტროლერის მეთოდისკენ
        await next();
    }
}