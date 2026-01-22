using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaTicketSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Version",
                table: "AspNetUsers",
                newName: "RowVersion");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Version",
                table: "Bookings",
                type: "datetime(6)",
                rowVersion: true,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp(6)",
                oldRowVersion: true,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "3686da2d-219f-4301-ad54-ed0fd7b647a8", "AQAAAAIAAYagAAAAEPPy2FDNz6xEnK3C0hUwUEQ6kcBFgqUpDBjfBZ41krNk6uDtNGhTusTlCUD/+inufg==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "AspNetUsers",
                newName: "Version");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Version",
                table: "Bookings",
                type: "timestamp(6)",
                rowVersion: true,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldRowVersion: true,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "cac1e795-b6f4-4f2e-9ecb-0fd314a79d1f", "AQAAAAIAAYagAAAAEBzxBNBzqmL34l41kDOodued5Z/2WtxGVSUJblY1i+U+VifzWLRhXzZqg4pBRRe01A==" });
        }
    }
}
