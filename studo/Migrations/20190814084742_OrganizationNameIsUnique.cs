using Microsoft.EntityFrameworkCore.Migrations;

namespace studo.Migrations
{
    public partial class OrganizationNameIsUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRightsInOrganizations_Organizations_OrganizationId",
                table: "UserRightsInOrganizations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRightsInOrganizations_OrganizationRights_OrganizationRightId",
                table: "UserRightsInOrganizations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRightsInOrganizations_AspNetUsers_UserId",
                table: "UserRightsInOrganizations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRightsInOrganizations",
                table: "UserRightsInOrganizations");

            migrationBuilder.RenameTable(
                name: "UserRightsInOrganizations",
                newName: "UserRightsInOrganization");

            migrationBuilder.RenameIndex(
                name: "IX_UserRightsInOrganizations_OrganizationRightId",
                table: "UserRightsInOrganization",
                newName: "IX_UserRightsInOrganization_OrganizationRightId");

            migrationBuilder.RenameIndex(
                name: "IX_UserRightsInOrganizations_OrganizationId",
                table: "UserRightsInOrganization",
                newName: "IX_UserRightsInOrganization_OrganizationId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Organizations",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRightsInOrganization",
                table: "UserRightsInOrganization",
                columns: new[] { "UserId", "OrganizationId", "OrganizationRightId" });

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name",
                table: "Organizations",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRightsInOrganization_Organizations_OrganizationId",
                table: "UserRightsInOrganization",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRightsInOrganization_OrganizationRights_OrganizationRightId",
                table: "UserRightsInOrganization",
                column: "OrganizationRightId",
                principalTable: "OrganizationRights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRightsInOrganization_AspNetUsers_UserId",
                table: "UserRightsInOrganization",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRightsInOrganization_Organizations_OrganizationId",
                table: "UserRightsInOrganization");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRightsInOrganization_OrganizationRights_OrganizationRightId",
                table: "UserRightsInOrganization");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRightsInOrganization_AspNetUsers_UserId",
                table: "UserRightsInOrganization");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_Name",
                table: "Organizations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRightsInOrganization",
                table: "UserRightsInOrganization");

            migrationBuilder.RenameTable(
                name: "UserRightsInOrganization",
                newName: "UserRightsInOrganizations");

            migrationBuilder.RenameIndex(
                name: "IX_UserRightsInOrganization_OrganizationRightId",
                table: "UserRightsInOrganizations",
                newName: "IX_UserRightsInOrganizations_OrganizationRightId");

            migrationBuilder.RenameIndex(
                name: "IX_UserRightsInOrganization_OrganizationId",
                table: "UserRightsInOrganizations",
                newName: "IX_UserRightsInOrganizations_OrganizationId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Organizations",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRightsInOrganizations",
                table: "UserRightsInOrganizations",
                columns: new[] { "UserId", "OrganizationId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserRightsInOrganizations_Organizations_OrganizationId",
                table: "UserRightsInOrganizations",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRightsInOrganizations_OrganizationRights_OrganizationRightId",
                table: "UserRightsInOrganizations",
                column: "OrganizationRightId",
                principalTable: "OrganizationRights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRightsInOrganizations_AspNetUsers_UserId",
                table: "UserRightsInOrganizations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
