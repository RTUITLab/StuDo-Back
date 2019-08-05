using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace studo.Migrations
{
    public partial class ReturnUserRightsInOrganizations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserRightsInOrganization",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    OrganizationId = table.Column<Guid>(nullable: false),
                    OrganizationRightId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRightsInOrganization", x => new { x.UserId, x.OrganizationId, x.OrganizationRightId });
                    table.ForeignKey(
                        name: "FK_UserRightsInOrganization_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRightsInOrganization_OrganizationRights_OrganizationRightId",
                        column: x => x.OrganizationRightId,
                        principalTable: "OrganizationRights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRightsInOrganization_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRightsInOrganization_OrganizationId",
                table: "UserRightsInOrganization",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRightsInOrganization_OrganizationRightId",
                table: "UserRightsInOrganization",
                column: "OrganizationRightId");

            migrationBuilder.DropTable(
                name: "UserRightsInOrganizations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
