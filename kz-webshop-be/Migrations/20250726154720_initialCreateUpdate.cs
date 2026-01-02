using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kz_webshop_be.Migrations
{
    /// <inheritdoc />
    public partial class initialCreateUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FunkoPop_IsUsed",
                table: "Product",
                newName: "IsExclusive");

            migrationBuilder.AlterColumn<int>(
                name: "Franchise",
                table: "Product",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AverageRating",
                table: "Product",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dimensions",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsChase",
                table: "Product",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNew",
                table: "Product",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnSale",
                table: "Product",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "Product",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Name",
                table: "FunkoPopTags",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "FunkoPopBadges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FunkoPopId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Badge = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunkoPopBadges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FunkoPopBadges_Product_FunkoPopId",
                        column: x => x.FunkoPopId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FunkoPopBadges_FunkoPopId",
                table: "FunkoPopBadges",
                column: "FunkoPopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FunkoPopBadges");

            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Dimensions",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "IsChase",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "IsNew",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "IsOnSale",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Product");

            migrationBuilder.RenameColumn(
                name: "IsExclusive",
                table: "Product",
                newName: "FunkoPop_IsUsed");

            migrationBuilder.AlterColumn<string>(
                name: "Franchise",
                table: "Product",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "FunkoPopTags",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
