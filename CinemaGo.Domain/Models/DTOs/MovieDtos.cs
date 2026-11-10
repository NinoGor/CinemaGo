namespace CinemaGo.Domain.Models.DTOs
{
    public record GenreResponse(int Id, string Name);

    public record HallResponse(int Id, string Name, int SeatCount);

    public record MovieRequest(
        string Title,
        string? Description,
        int DurationMinutes,
        int AgeRating,
        string? PosterUrl,
        List<int> GenreIds);

    public record MovieResponse(
        int Id,
        string Title,
        string? Description,
        int DurationMinutes,
        int AgeRating,
        string? PosterUrl,
        IReadOnlyList<GenreResponse> Genres);

    public record PosterUrlRequest(string? PosterUrl);

    public record GenreRequest(string Name);
    public record HallCreateRequest(string Name, int Rows, int SeatsPerRow, int? FirstVipRow);
    public record HallUpdateRequest(string Name);
    public record SeatResponse(int Id, int RowNumber, int SeatNumber, bool IsVip);
    public record SeatUpdateRequest(bool IsVip);
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
