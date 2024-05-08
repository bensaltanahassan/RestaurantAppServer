using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantAppServer.Migrations
{
    /// <inheritdoc />
    public partial class AddedEmailAndNumberForDeliveryUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "DeliveryMen",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "DeliveryMen",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "DeliveryMen");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "DeliveryMen");
        }
    }
}
