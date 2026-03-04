using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GitHubLogin",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GitHubUserId",
                table: "users",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GitHubLogin",
                table: "users");

            migrationBuilder.DropColumn(
                name: "GitHubUserId",
                table: "users");
        }
    }
}
