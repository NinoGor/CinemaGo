namespace CinemaGo.Domain.Models.DTOs
{
    public record RegisterRequest(string UserName, string Email, string Password);

    // ადმინის რეგისტრაციას დამატებით სჭირდება secret key 
    public record RegisterAdminRequest(string UserName, string Email, string Password, string AdminSecret);

    public record LoginRequest(string Email, string Password);

    // "ჩვეულებრივი" მომხმარებლები რეგისტრაციისას არ მიიღებენ ტოკენს, ჯერ მეილის ვერიფიკაცია უნდა მოხდეს
    public record RegisterResponse(int UserId, string Email, string Message);

    public record ConfirmEmailRequest(int UserId, string Token);

    public record ResendConfirmationRequest(string Email);

    public record MessageResponse(string Message);

    public record AuthResponse(
        int UserId,
        string UserName,
        string Email,
        IReadOnlyList<string> Roles,
        string Token,
        DateTime ExpiresAt);
}
