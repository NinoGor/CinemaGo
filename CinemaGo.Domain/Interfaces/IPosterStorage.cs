namespace CinemaGo.Domain.Interfaces
{
    public interface IPosterStorage
    {
        Task<string> SaveAsync(byte[] content, string extension, CancellationToken ct = default);
        Task DeleteAsync(string? publicPath, CancellationToken ct = default);
        bool IsManaged(string? publicPath);
    }
}
