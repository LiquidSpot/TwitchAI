using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwitchAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initialize_project : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "twitch-ai-client");

            migrationBuilder.CreateTable(
                name: "RequestModel",
                schema: "twitch-ai-client",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    model = table.Column<string>(type: "text", nullable: false),
                    max_tokens = table.Column<int>(type: "integer", nullable: true),
                    temperature = table.Column<double>(type: "double precision", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TwitchUser",
                schema: "twitch-ai-client",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TwitchId = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwitchUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usage",
                schema: "twitch-ai-client",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    prompt_tokens = table.Column<int>(type: "integer", nullable: false),
                    completion_tokens = table.Column<int>(type: "integer", nullable: false),
                    total_tokens = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserMessage",
                schema: "twitch-ai-client",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    temp = table.Column<double>(type: "double precision", nullable: true),
                    maxToken = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MessageGpt",
                schema: "twitch-ai-client",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "text", nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    RequestModelId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageGpt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageGpt_RequestModel_RequestModelId",
                        column: x => x.RequestModelId,
                        principalSchema: "twitch-ai-client",
                        principalTable: "RequestModel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChatMessage",
                schema: "twitch-ai-client",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TwitchUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessage_TwitchUser_TwitchUserId",
                        column: x => x.TwitchUserId,
                        principalSchema: "twitch-ai-client",
                        principalTable: "TwitchUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TextCompletion",
                schema: "twitch-ai-client",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    @object = table.Column<string>(name: "object", type: "text", nullable: false),
                    created = table.Column<int>(type: "integer", nullable: false),
                    model = table.Column<string>(type: "text", nullable: false),
                    IdUsage = table.Column<int>(type: "integer", nullable: false),
                    UsageId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextCompletion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TextCompletion_Usage_UsageId",
                        column: x => x.UsageId,
                        principalSchema: "twitch-ai-client",
                        principalTable: "Usage",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Choice",
                schema: "twitch-ai-client",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    FinishReason = table.Column<string>(type: "text", nullable: true),
                    TextCompletionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Choice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Choice_TextCompletion_TextCompletionId",
                        column: x => x.TextCompletionId,
                        principalSchema: "twitch-ai-client",
                        principalTable: "TextCompletion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessage_TwitchUserId",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                column: "TwitchUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Choice_TextCompletionId",
                schema: "twitch-ai-client",
                table: "Choice",
                column: "TextCompletionId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageGpt_RequestModelId",
                schema: "twitch-ai-client",
                table: "MessageGpt",
                column: "RequestModelId");

            migrationBuilder.CreateIndex(
                name: "IX_TextCompletion_UsageId",
                schema: "twitch-ai-client",
                table: "TextCompletion",
                column: "UsageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessage",
                schema: "twitch-ai-client");

            migrationBuilder.DropTable(
                name: "Choice",
                schema: "twitch-ai-client");

            migrationBuilder.DropTable(
                name: "MessageGpt",
                schema: "twitch-ai-client");

            migrationBuilder.DropTable(
                name: "UserMessage",
                schema: "twitch-ai-client");

            migrationBuilder.DropTable(
                name: "TwitchUser",
                schema: "twitch-ai-client");

            migrationBuilder.DropTable(
                name: "TextCompletion",
                schema: "twitch-ai-client");

            migrationBuilder.DropTable(
                name: "RequestModel",
                schema: "twitch-ai-client");

            migrationBuilder.DropTable(
                name: "Usage",
                schema: "twitch-ai-client");
        }
    }
}
