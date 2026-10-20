namespace CinemaGo.Domain.Rules
{
    public static class SessionRules
    {
        // იმის შესამოწმებლად გადაფარვა თუა სესიებს შორის
        // [start, end) ინტერვალები; სესია შეიძლება დაიწყოს იმ დროს როდესაც წინა მთავრდება
        public static bool Overlaps(DateTime firstStart, int firstDurationMinutes, DateTime secondStart, int secondDurationMinutes)
        {
            if (firstDurationMinutes <= 0 || secondDurationMinutes <= 0)
                throw new ArgumentOutOfRangeException(nameof(firstDurationMinutes), "Durations must be positive.");
            var firstEnd = firstStart.AddMinutes(firstDurationMinutes);
            var secondEnd = secondStart.AddMinutes(secondDurationMinutes);
            return firstStart < secondEnd && secondStart < firstEnd;
        }
    }
}
