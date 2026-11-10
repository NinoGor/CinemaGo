using System.Text.RegularExpressions;
using CinemaGo.Domain.Interfaces;

namespace CinemaGo.Infrastructure;

public partial class LocalPosterStorage : IPosterStorage
{
    private const string PublicPrefix = "/posters/";

    // Only names we generate ourselves (32 hex characters + a known extension) are ever touched.
    // This also makes path traversal ("/posters/../x") impossible.
    [GeneratedRegex(@"^/posters/(?<name>[0-9a-f]{32}\.(jpg|png|webp))$")]
    private static partial Regex ManagedPath();

    private readonly string _folder;

    public LocalPosterStorage(string folder)
    {
        _folder = folder;
        Directory.CreateDirectory(_folder);
    }

    public async Task<string> SaveAsync(byte[] content, string extension, CancellationToken ct = default)
    {
        var name = Guid.NewGuid().ToString("N") + extension;
        await File.WriteAllBytesAsync(Path.Combine(_folder, name), content, ct);
        return PublicPrefix + name;
    }

    public Task DeleteAsync(string? publicPath, CancellationToken ct = default)
    {
        var match = ManagedPath().Match(publicPath ?? string.Empty);
        if (match.Success)
        {
            var file = Path.Combine(_folder, match.Groups["name"].Value);
            if (File.Exists(file))
                File.Delete(file);
        }

        return Task.CompletedTask;
    }

    public bool IsManaged(string? publicPath) => ManagedPath().IsMatch(publicPath ?? string.Empty);
}
