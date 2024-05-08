using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantAppServer.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToDeliveryManModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "DeliveryMen",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "DeliveryMen");
        }
    }
}
