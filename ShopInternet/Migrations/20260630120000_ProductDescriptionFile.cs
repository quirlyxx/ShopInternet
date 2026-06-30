using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopInternet.Migrations
{
    /// <inheritdoc />
    public partial class ProductDescriptionFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionFile",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionFile",
                table: "Product");
        }
    }
}
