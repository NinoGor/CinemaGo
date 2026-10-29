namespace CinemaGo.Domain.Models.DTOs
{
    public record GenreResponse(int Id, string Name);

    public record HallResponse(int Id, string Name, int SeatCount);

    public record MovieRequest(
        string Title,
        string? Description,
        int DurationMinutes,
        int AgeRating,
        List<int> GenreIds);

    public record MovieResponse(
        int Id,
        string Title,
        string? Description,
        int DurationMinutes,
        int AgeRating,
        IReadOnlyList<GenreResponse> Genres);

    public class MovieFilter : PaginationParams
    {
        public string? Title { get; set; }
        public int? GenreId { get; set; }
        public int? MaxAgeRating { get; set; }

        // "title" იქნება "გაჩუმებით" ან "duration"
        public string? SortBy { get; set; }
        public bool Descending { get; set; }
    }

}
