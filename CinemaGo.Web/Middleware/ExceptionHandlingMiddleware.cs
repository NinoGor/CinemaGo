using CinemaGo.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CinemaGo.Web.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // აქ ვამატებთ შენს დომენურ გამონაკლისებს, რომ მიდლვეარმა ზუსტად იცოდეს რომელი სტატუსი მიანიჭოს
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            BusinessRuleException => (StatusCodes.Status422UnprocessableEntity, "Business rule violation"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad request"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Bad request"),
            DbUpdateException => (StatusCodes.Status409Conflict, "A database conflict occurred. The record might already exist."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (exception is DomainException)
            _logger.LogWarning("Handled {ExceptionType}: {Message}", exception.GetType().Name, exception.Message);
        else
            _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}",
                context.Request.Method, context.Request.Path);

        // რადგან შენ გინდა რომ DomainException-ების დროს ზუსტი შეტყობინება (მაგ: "Invalid email or password.") მივიდეს კლიენტთან:
        var message = exception is DomainException || statusCode != StatusCodes.Status500InternalServerError
            ? exception.Message
            : "Please try again later or contact support.";

        var response = new
        {
            success = false,
            message = message,
            status = statusCode,
            traceId = context.TraceIdentifier
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(response);
    }
}