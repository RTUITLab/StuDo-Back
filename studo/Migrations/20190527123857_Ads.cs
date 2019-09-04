using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace studo.Migrations
{
    public partial class Ads : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "CreatorType",
                table: "Ads");

            migrationBuilder.AddColumn<DateTime>(
                name: "BeginTime",
                table: "Ads",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "Ads",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Ads",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Avatar = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ads_OrganizationId",
                table: "Ads",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ads_Organization_OrganizationId",
                table: "Ads",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ads_Organization_OrganizationId",
                table: "Ads");

            migrationBuilder.DropTable(
                name: "Organization");

            migrationBuilder.DropIndex(
                name: "IX_Ads_OrganizationId",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "BeginTime",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Ads");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Ads",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "CreatorType",
                table: "Ads",
                nullable: false,
                defaultValue: 0);
        }
    }
}
