using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwitchAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_UserApp_Integration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUser",
                schema: "twitch-ai-client",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    PasswordSalt = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BotSettings",
                schema: "twitch-ai-client",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefaultRole = table.Column<string>(type: "text", nullable: false),
                    CooldownSeconds = table.Column<int>(type: "integer", nullable: false),
                    ReplyLimit = table.Column<int>(type: "integer", nullable: false),
                    EnableAi = table.Column<bool>(type: "boolean", nullable: false),
                    EnableCompliment = table.Column<bool>(type: "boolean", nullable: false),
                    EnableFact = table.Column<bool>(type: "boolean", nullable: false),
                    EnableHoliday = table.Column<bool>(type: "boolean", nullable: false),
                    EnableTranslation = table.Column<bool>(type: "boolean", nullable: false),
                    EnableSoundAlerts = table.Column<bool>(type: "boolean", nullable: false),
                    EnableViewersStats = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BotSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                schema: "twitch-ai-client",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserIntegrationSettings",
                schema: "twitch-ai-client",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TwitchChannelName = table.Column<string>(type: "text", nullable: true),
                    TwitchBotUsername = table.Column<string>(type: "text", nullable: true),
                    TwitchAccessTokenEncrypted = table.Column<string>(type: "text", nullable: true),
                    TwitchRefreshTokenEncrypted = table.Column<string>(type: "text", nullable: true),
                    TwitchClientId = table.Column<string>(type: "text", nullable: true),
                    OpenAiOrganizationId = table.Column<string>(type: "text", nullable: true),
                    OpenAiProjectId = table.Column<string>(type: "text", nullable: true),
                    OpenAiApiKeyEncrypted = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserIntegrationSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUser",
                schema: "twitch-ai-client");

            migrationBuilder.DropTable(
                name: "BotSettings",
                schema: "twitch-ai-client");

            migrationBuilder.DropTable(
                name: "RefreshToken",
                schema: "twitch-ai-client");

            migrationBuilder.DropTable(
                name: "UserIntegrationSettings",
                schema: "twitch-ai-client");
        }
    }
}
