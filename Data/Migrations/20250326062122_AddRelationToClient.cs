using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoot.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationToClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "ClientRedirectUri",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "ClientPostLogoutRedirectUri",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "ClientCorsOrigin",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ClientSecret_ClientId",
                table: "ClientSecret",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientScope_ClientId",
                table: "ClientScope",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRedirectUri_ClientId",
                table: "ClientRedirectUri",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientPostLogoutRedirectUri_ClientId",
                table: "ClientPostLogoutRedirectUri",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientCorsOrigin_ClientId",
                table: "ClientCorsOrigin",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientCorsOrigin_Client_ClientId",
                table: "ClientCorsOrigin",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientPostLogoutRedirectUri_Client_ClientId",
                table: "ClientPostLogoutRedirectUri",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRedirectUri_Client_ClientId",
                table: "ClientRedirectUri",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientScope_Client_ClientId",
                table: "ClientScope",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientSecret_Client_ClientId",
                table: "ClientSecret",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientCorsOrigin_Client_ClientId",
                table: "ClientCorsOrigin");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientPostLogoutRedirectUri_Client_ClientId",
                table: "ClientPostLogoutRedirectUri");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientRedirectUri_Client_ClientId",
                table: "ClientRedirectUri");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientScope_Client_ClientId",
                table: "ClientScope");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientSecret_Client_ClientId",
                table: "ClientSecret");

            migrationBuilder.DropIndex(
                name: "IX_ClientSecret_ClientId",
                table: "ClientSecret");

            migrationBuilder.DropIndex(
                name: "IX_ClientScope_ClientId",
                table: "ClientScope");

            migrationBuilder.DropIndex(
                name: "IX_ClientRedirectUri_ClientId",
                table: "ClientRedirectUri");

            migrationBuilder.DropIndex(
                name: "IX_ClientPostLogoutRedirectUri_ClientId",
                table: "ClientPostLogoutRedirectUri");

            migrationBuilder.DropIndex(
                name: "IX_ClientCorsOrigin_ClientId",
                table: "ClientCorsOrigin");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "ClientRedirectUri");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "ClientPostLogoutRedirectUri");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "ClientCorsOrigin");
        }
    }
}
