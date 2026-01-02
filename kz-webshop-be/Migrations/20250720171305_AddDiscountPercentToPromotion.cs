using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kz_webshop_be.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountPercentToPromotion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Promotions");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "Promotions",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Promotions");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Promotions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
