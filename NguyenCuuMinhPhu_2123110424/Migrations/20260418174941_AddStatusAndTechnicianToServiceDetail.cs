using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NguyenCuuMinhPhu_2123110424.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusAndTechnicianToServiceDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "RepairOrderServiceDetails",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "RepairOrderServiceDetails");
        }
    }
}
