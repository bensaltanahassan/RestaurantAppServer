using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantAppServer.Migrations
{
    /// <inheritdoc />
    public partial class makeFullNameInsteadOfFirstNameAndLastNameInReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Reservations",
                newName: "FullName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Reservations",
                newName: "LastName");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
