using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CinemaGo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSeededSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Sessions",
                columns: new[] { "Id", "EndsAt", "HallId", "MovieId", "StandardPrice", "StartsAt", "Status", "VipPrice" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 11, 10, 16, 30, 0, 0, DateTimeKind.Utc), 1, 1, 15.00m, new DateTime(2026, 11, 10, 14, 0, 0, 0, DateTimeKind.Utc), 0, 25.00m },
                    { 2, new DateTime(2026, 11, 10, 19, 36, 0, 0, DateTimeKind.Utc), 2, 2, 18.00m, new DateTime(2026, 11, 10, 17, 0, 0, 0, DateTimeKind.Utc), 0, 30.00m },
                    { 3, new DateTime(2026, 11, 10, 22, 51, 0, 0, DateTimeKind.Utc), 1, 4, 20.00m, new DateTime(2026, 11, 10, 20, 0, 0, 0, DateTimeKind.Utc), 0, 35.00m },
                    { 4, new DateTime(2026, 11, 11, 14, 5, 59, 999, DateTimeKind.Utc).AddTicks(9999), 2, 3, 12.00m, new DateTime(2026, 11, 11, 12, 0, 0, 0, DateTimeKind.Utc), 0, 20.00m },
                    { 5, new DateTime(2026, 11, 11, 17, 30, 0, 0, DateTimeKind.Utc), 2, 1, 15.00m, new DateTime(2026, 11, 11, 15, 0, 0, 0, DateTimeKind.Utc), 0, 25.00m },
                    { 6, new DateTime(2026, 11, 11, 20, 36, 0, 0, DateTimeKind.Utc), 1, 2, 18.00m, new DateTime(2026, 11, 11, 18, 0, 0, 0, DateTimeKind.Utc), 0, 30.00m },
                    { 7, new DateTime(2026, 11, 12, 18, 51, 0, 0, DateTimeKind.Utc), 1, 4, 20.00m, new DateTime(2026, 11, 12, 16, 0, 0, 0, DateTimeKind.Utc), 0, 35.00m },
                    { 8, new DateTime(2026, 11, 12, 21, 6, 0, 0, DateTimeKind.Utc), 2, 3, 14.00m, new DateTime(2026, 11, 12, 19, 0, 0, 0, DateTimeKind.Utc), 0, 22.00m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Sessions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Sessions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Sessions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Sessions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Sessions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Sessions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Sessions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Sessions",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
