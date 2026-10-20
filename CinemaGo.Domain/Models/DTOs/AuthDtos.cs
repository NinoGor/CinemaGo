using System.ComponentModel.DataAnnotations;
namespace CinemaGo.Domain.Models.DTOs
{
    public sealed class RegisterRequest
    {
        [Required, StringLength(60, MinimumLength = 3)] public string UserName { get; set; } = string.Empty;
        [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
        [Required, MinLength(8), StringLength(100)] public string Password { get; set; } = string.Empty;
    }
    public sealed class LoginRequest
    {
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }
    public sealed record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, UserDto User);
    public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
}
