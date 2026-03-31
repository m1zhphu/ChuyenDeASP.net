using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NguyenCuuMinhPhu_2123110424.Migrations
{
    /// <inheritdoc />
    public partial class AddVinNumberToVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VinNumber",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VinNumber",
                table: "Vehicles");
        }
    }
}
