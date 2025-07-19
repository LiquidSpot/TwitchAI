using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwitchAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Reply_Fields_To_ChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReply",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReplyParentMessageId",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReply",
                schema: "twitch-ai-client",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "ReplyParentMessageId",
                schema: "twitch-ai-client",
                table: "ChatMessage");
        }
    }
}
