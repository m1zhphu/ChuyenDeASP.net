using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NguyenCuuMinhPhu_2123110424.Migrations
{
    /// <inheritdoc />
    public partial class AddTimingToServiceDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActualEndTime",
                table: "RepairOrderServiceDetails",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualStartTime",
                table: "RepairOrderServiceDetails",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderServiceDetails_MechanicId",
                table: "RepairOrderServiceDetails",
                column: "MechanicId");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderServiceDetails_Users_MechanicId",
                table: "RepairOrderServiceDetails",
                column: "MechanicId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrderServiceDetails_Users_MechanicId",
                table: "RepairOrderServiceDetails");

            migrationBuilder.DropIndex(
                name: "IX_RepairOrderServiceDetails_MechanicId",
                table: "RepairOrderServiceDetails");

            migrationBuilder.DropColumn(
                name: "ActualEndTime",
                table: "RepairOrderServiceDetails");

            migrationBuilder.DropColumn(
                name: "ActualStartTime",
                table: "RepairOrderServiceDetails");
        }
    }
}
