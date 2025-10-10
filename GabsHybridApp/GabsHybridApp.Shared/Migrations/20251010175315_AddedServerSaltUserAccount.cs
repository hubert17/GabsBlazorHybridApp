using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GabsHybridApp.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddedServerSaltUserAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServerSalt",
                table: "UserAccounts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServerSalt",
                table: "UserAccounts");
        }
    }
}
