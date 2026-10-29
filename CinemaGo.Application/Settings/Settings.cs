namespace CinemaGo.Application.Settings
{
    public class JwtSettings
    {
        public const string SectionName = "Jwt";

        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; } = 60;
    }

    // JWT ტოკენის ქლეიმების უნივერსალური სახელები (მოკლე ფორმატში, რომ .NET-მა თავისით არ გადააკეთოს)
    public static class AppClaimTypes
    {
        public const string UserId = "sub"; // subject
        public const string UserName = "unique_name";
        public const string Email = "email";
        public const string Role = "role";
    }

    public class AdminRegistrationSettings
    {
        public const string SectionName = "AdminRegistration";

        // Shared secret POST-ისთვის: /api/auth/register-admin. მაქვს appsettings.Local.json-ში
        public string SecretKey { get; set; } = string.Empty;
    }

    public class EmailSettings
    {
        public const string SectionName = "Email";

        // გაჩუმებით არის "Simulated" (მხოლოდ ლოგირებისთვის) ან "Smtp" (რეალური მეილებისთვის)
        public string Provider { get; set; } = "Simulated";

        // API-ის Base URL, confirmation link-ის ასაწყობად
        public string PublicBaseUrl { get; set; } = "https://localhost:7080";

        public SmtpSettings Smtp { get; set; } = new();
    }

    public class SmtpSettings
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string UserName { get; set; } = string.Empty;

        // Gmail-ის შემთხვევაში არის appsettings.Local.json-ში
        public string Password { get; set; } = string.Empty;

        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = "CinemaGo";
    }
}
