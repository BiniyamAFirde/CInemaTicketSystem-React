using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaTicketSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixScreeningBookingCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Screenings_ScreeningId",
                table: "Bookings");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "09947d4c-b37e-4a49-aa8e-64e752a73655", "AQAAAAIAAYagAAAAED0H7i2hZ+B2Ett3px0zgtjVR8f0jx8CueN4/yNwuYWIcSykrbBKkJ8ms+3zF/uW/g==" });

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Screenings_ScreeningId",
                table: "Bookings",
                column: "ScreeningId",
                principalTable: "Screenings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Screenings_ScreeningId",
                table: "Bookings");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "6ffe87b7-6a4b-4fa2-9c1d-6125219d52a6", "AQAAAAIAAYagAAAAEBNnwhkUr6FZ4elvI1jL3+fPI3lhGUUMQHf9ZXPt2qKTFkAe+j57DA3EVb7Hi/jZeA==" });

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Screenings_ScreeningId",
                table: "Bookings",
                column: "ScreeningId",
                principalTable: "Screenings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
