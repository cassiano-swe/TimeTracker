using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGitHubIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "github_integrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstallationId = table.Column<long>(type: "bigint", nullable: false),
                    AccountLogin = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AccountType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_github_integrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "github_repositories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    GitHubIntegrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    GitHubRepositoryId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FullName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DefaultBranch = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_github_repositories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_github_integrations_InstallationId",
                table: "github_integrations",
                column: "InstallationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_github_integrations_WorkspaceId",
                table: "github_integrations",
                column: "WorkspaceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_github_repositories_GitHubIntegrationId_GitHubRepositoryId",
                table: "github_repositories",
                columns: new[] { "GitHubIntegrationId", "GitHubRepositoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_github_repositories_GitHubRepositoryId",
                table: "github_repositories",
                column: "GitHubRepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_github_repositories_WorkspaceId_FullName",
                table: "github_repositories",
                columns: new[] { "WorkspaceId", "FullName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "github_integrations");

            migrationBuilder.DropTable(
                name: "github_repositories");
        }
    }
}
