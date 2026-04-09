using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NguyenCuuMinhPhu_2123110424.Migrations
{
    /// <inheritdoc />
    public partial class AddNewModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrderParts_Parts_PartId",
                table: "RepairOrderParts");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrderParts_RepairOrders_RepairOrderId",
                table: "RepairOrderParts");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrders_Vehicles_VehicleId",
                table: "RepairOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrderServices_RepairOrders_RepairOrderId",
                table: "RepairOrderServices");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrderServices_Services_ServiceId",
                table: "RepairOrderServices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RepairOrderServices",
                table: "RepairOrderServices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RepairOrderParts",
                table: "RepairOrderParts");

            migrationBuilder.RenameTable(
                name: "RepairOrderServices",
                newName: "RepairOrderServiceDetails");

            migrationBuilder.RenameTable(
                name: "RepairOrderParts",
                newName: "RepairOrderPartDetails");

            migrationBuilder.RenameIndex(
                name: "IX_RepairOrderServices_ServiceId",
                table: "RepairOrderServiceDetails",
                newName: "IX_RepairOrderServiceDetails_ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_RepairOrderServices_RepairOrderId",
                table: "RepairOrderServiceDetails",
                newName: "IX_RepairOrderServiceDetails_RepairOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_RepairOrderParts_RepairOrderId",
                table: "RepairOrderPartDetails",
                newName: "IX_RepairOrderPartDetails_RepairOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_RepairOrderParts_PartId",
                table: "RepairOrderPartDetails",
                newName: "IX_RepairOrderPartDetails_PartId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RepairOrderServiceDetails",
                table: "RepairOrderServiceDetails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RepairOrderPartDetails",
                table: "RepairOrderPartDetails",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedServices = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TaxCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoucherCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsageLimit = table.Column<int>(type: "int", nullable: false),
                    UsedCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vouchers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiptCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryReceipts_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryReceiptDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ImportPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReceiptDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryReceiptDetails_InventoryReceipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "InventoryReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryReceiptDetails_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_OrderCode",
                table: "RepairOrders",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CustomerId",
                table: "Appointments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptDetails_PartId",
                table: "InventoryReceiptDetails",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceiptDetails_ReceiptId",
                table: "InventoryReceiptDetails",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipts_ReceiptCode",
                table: "InventoryReceipts",
                column: "ReceiptCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReceipts_SupplierId",
                table: "InventoryReceipts",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderPartDetails_Parts_PartId",
                table: "RepairOrderPartDetails",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderPartDetails_RepairOrders_RepairOrderId",
                table: "RepairOrderPartDetails",
                column: "RepairOrderId",
                principalTable: "RepairOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrders_Vehicles_VehicleId",
                table: "RepairOrders",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderServiceDetails_RepairOrders_RepairOrderId",
                table: "RepairOrderServiceDetails",
                column: "RepairOrderId",
                principalTable: "RepairOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderServiceDetails_Services_ServiceId",
                table: "RepairOrderServiceDetails",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrderPartDetails_Parts_PartId",
                table: "RepairOrderPartDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrderPartDetails_RepairOrders_RepairOrderId",
                table: "RepairOrderPartDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrders_Vehicles_VehicleId",
                table: "RepairOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrderServiceDetails_RepairOrders_RepairOrderId",
                table: "RepairOrderServiceDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrderServiceDetails_Services_ServiceId",
                table: "RepairOrderServiceDetails");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "InventoryReceiptDetails");

            migrationBuilder.DropTable(
                name: "Vouchers");

            migrationBuilder.DropTable(
                name: "InventoryReceipts");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_RepairOrders_OrderCode",
                table: "RepairOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RepairOrderServiceDetails",
                table: "RepairOrderServiceDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RepairOrderPartDetails",
                table: "RepairOrderPartDetails");

            migrationBuilder.RenameTable(
                name: "RepairOrderServiceDetails",
                newName: "RepairOrderServices");

            migrationBuilder.RenameTable(
                name: "RepairOrderPartDetails",
                newName: "RepairOrderParts");

            migrationBuilder.RenameIndex(
                name: "IX_RepairOrderServiceDetails_ServiceId",
                table: "RepairOrderServices",
                newName: "IX_RepairOrderServices_ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_RepairOrderServiceDetails_RepairOrderId",
                table: "RepairOrderServices",
                newName: "IX_RepairOrderServices_RepairOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_RepairOrderPartDetails_RepairOrderId",
                table: "RepairOrderParts",
                newName: "IX_RepairOrderParts_RepairOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_RepairOrderPartDetails_PartId",
                table: "RepairOrderParts",
                newName: "IX_RepairOrderParts_PartId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RepairOrderServices",
                table: "RepairOrderServices",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RepairOrderParts",
                table: "RepairOrderParts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderParts_Parts_PartId",
                table: "RepairOrderParts",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderParts_RepairOrders_RepairOrderId",
                table: "RepairOrderParts",
                column: "RepairOrderId",
                principalTable: "RepairOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrders_Vehicles_VehicleId",
                table: "RepairOrders",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderServices_RepairOrders_RepairOrderId",
                table: "RepairOrderServices",
                column: "RepairOrderId",
                principalTable: "RepairOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderServices_Services_ServiceId",
                table: "RepairOrderServices",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
