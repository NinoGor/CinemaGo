namespace CinemaGo.Domain.Models.DTOs
{
    // პაგინაცია (საერთო)
    public class PaginationParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        // ითვლის რამდენი გამოტოვება უნდა მოხდეს, რომ მივიდეთ მიმდინარე გვერდამდე
        public int Skip => (Page - 1) * PageSize;
    }

    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public PagedResult<TOut> Map<TOut>(Func<T, TOut> selector) => new()
        {
            Items = Items.Select(selector).ToList(),
            TotalCount = TotalCount,
            Page = Page,
            PageSize = PageSize
        };
    }
}