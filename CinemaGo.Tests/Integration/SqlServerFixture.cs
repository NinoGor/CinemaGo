using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Tests.Integration;

// თუ მიგრაციები არსებობს განახორციელებს, სხვა შემთხვევაში EnsureCreated აგებს სქემას
public class SqlServerFixture : IAsyncLifetime
{
    // ცალკეული ბაზა, ტესტირებები რომ დეველოპმენტის მონაცემებს არ შეეხოს
    private const string DefaultConnection =
        "Server=(localdb)\\MSSQLLocalDB;Database=CinemaGoDB_Tests;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

    public string ConnectionString { get; } =
        Environment.GetEnvironmentVariable("CINEMAGO_TEST_CONNECTION") ?? DefaultConnection;

    public CinemaGoDbContext CreateContext()
        => new(new DbContextOptionsBuilder<CinemaGoDbContext>().UseSqlServer(ConnectionString).Options);

    public async Task InitializeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();

        if (context.Database.GetMigrations().Any())
            await context.Database.MigrateAsync();
        else
            await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
    }
}
