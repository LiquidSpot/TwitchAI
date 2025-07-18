using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwitchAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_ViewerPresence_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ViewerPresence",
                schema: "twitch-ai-client",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TwitchUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelName = table.Column<string>(type: "text", nullable: false),
                    LastSeenInChat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSilent = table.Column<bool>(type: "boolean", nullable: false),
                    TotalPresenceMinutes = table.Column<int>(type: "integer", nullable: false),
                    SessionCount = table.Column<int>(type: "integer", nullable: false),
                    CurrentSessionStarted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MetadataJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerPresence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ViewerPresence_TwitchUser_TwitchUserId",
                        column: x => x.TwitchUserId,
                        principalSchema: "twitch-ai-client",
                        principalTable: "TwitchUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ViewerPresence_TwitchUserId",
                schema: "twitch-ai-client",
                table: "ViewerPresence",
                column: "TwitchUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViewerPresence",
                schema: "twitch-ai-client");
        }
    }
}
