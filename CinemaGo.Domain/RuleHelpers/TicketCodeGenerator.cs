namespace CinemaGo.Domain.Rules
{
    public static class TicketCodeGenerator
    {
        // :N აიდის მოაშორებს ტირეებს. გადაგვყავს uppercase-ში
        public static string Generate()
        {
            var datePart = DateTime.UtcNow.ToString("yyMMdd");
            return $"TKN-{datePart}-{Guid.NewGuid():N}".ToUpperInvariant();
        }
    }
}
