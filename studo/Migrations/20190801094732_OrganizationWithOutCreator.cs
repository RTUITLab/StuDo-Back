using Microsoft.EntityFrameworkCore.Migrations;

namespace studo.Migrations
{
    public partial class OrganizationWithOutCreator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "Organizations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "Organizations",
                nullable: true);
        }
    }
}
