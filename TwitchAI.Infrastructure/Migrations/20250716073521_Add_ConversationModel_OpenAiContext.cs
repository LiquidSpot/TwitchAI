using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwitchAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_ConversationModel_OpenAiContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConversationMessage",
                schema: "twitch-ai-client",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TwitchUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    MessageOrder = table.Column<int>(type: "integer", nullable: false),
                    OpenAiResponseId = table.Column<string>(type: "text", nullable: true),
                    OpenAiModel = table.Column<string>(type: "text", nullable: true),
                    TokenCount = table.Column<int>(type: "integer", nullable: true),
                    ChatMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationMessage_ChatMessage_ChatMessageId",
                        column: x => x.ChatMessageId,
                        principalSchema: "twitch-ai-client",
                        principalTable: "ChatMessage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConversationMessage_TwitchUser_TwitchUserId",
                        column: x => x.TwitchUserId,
                        principalSchema: "twitch-ai-client",
                        principalTable: "TwitchUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMessage_ChatMessageId",
                schema: "twitch-ai-client",
                table: "ConversationMessage",
                column: "ChatMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMessage_TwitchUserId",
                schema: "twitch-ai-client",
                table: "ConversationMessage",
                column: "TwitchUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationMessage",
                schema: "twitch-ai-client");
        }
    }
}
