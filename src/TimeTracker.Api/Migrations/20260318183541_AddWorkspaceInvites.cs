using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceInvites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workspace_invites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Token = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workspace_invites", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_workspace_invites_Token",
                table: "workspace_invites",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workspace_invites_WorkspaceId_Email",
                table: "workspace_invites",
                columns: new[] { "WorkspaceId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_workspace_invites_WorkspaceId_Status",
                table: "workspace_invites",
                columns: new[] { "WorkspaceId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workspace_invites");
        }
    }
}
