using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VendorHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "ParentCategoryId" },
                values: new object[] { 1000, "Others", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1000);
        }
    }
}
