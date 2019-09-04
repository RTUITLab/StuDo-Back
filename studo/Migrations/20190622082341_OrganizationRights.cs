using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace studo.Migrations
{
    public partial class OrganizationRights : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganizationRights",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RightName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationRights", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRightsInOrganizations",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    OrganizationId = table.Column<Guid>(nullable: false),
                    OrganizationRightId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRightsInOrganizations", x => new { x.UserId, x.OrganizationId });
                    table.ForeignKey(
                        name: "FK_UserRightsInOrganizations_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRightsInOrganizations_OrganizationRights_OrganizationRightId",
                        column: x => x.OrganizationRightId,
                        principalTable: "OrganizationRights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRightsInOrganizations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRightsInOrganizations_OrganizationId",
                table: "UserRightsInOrganizations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRightsInOrganizations_OrganizationRightId",
                table: "UserRightsInOrganizations",
                column: "OrganizationRightId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRightsInOrganizations");

            migrationBuilder.DropTable(
                name: "OrganizationRights");
        }
    }
}
