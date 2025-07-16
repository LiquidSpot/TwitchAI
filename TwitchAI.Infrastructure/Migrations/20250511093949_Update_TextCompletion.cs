using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwitchAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_TextCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Choice_TextCompletion_TextCompletionId",
                schema: "twitch-ai-client",
                table: "Choice");

            migrationBuilder.AddColumn<Guid>(
                name: "TwitchUserId",
                schema: "twitch-ai-client",
                table: "UserMessage",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TwitchUserId",
                schema: "twitch-ai-client",
                table: "TextCompletion",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "TextCompletionId",
                schema: "twitch-ai-client",
                table: "Choice",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMessage_TwitchUserId",
                schema: "twitch-ai-client",
                table: "UserMessage",
                column: "TwitchUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TextCompletion_TwitchUserId",
                schema: "twitch-ai-client",
                table: "TextCompletion",
                column: "TwitchUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Choice_TextCompletion_TextCompletionId",
                schema: "twitch-ai-client",
                table: "Choice",
                column: "TextCompletionId",
                principalSchema: "twitch-ai-client",
                principalTable: "TextCompletion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TextCompletion_TwitchUser_TwitchUserId",
                schema: "twitch-ai-client",
                table: "TextCompletion",
                column: "TwitchUserId",
                principalSchema: "twitch-ai-client",
                principalTable: "TwitchUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMessage_TwitchUser_TwitchUserId",
                schema: "twitch-ai-client",
                table: "UserMessage",
                column: "TwitchUserId",
                principalSchema: "twitch-ai-client",
                principalTable: "TwitchUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Choice_TextCompletion_TextCompletionId",
                schema: "twitch-ai-client",
                table: "Choice");

            migrationBuilder.DropForeignKey(
                name: "FK_TextCompletion_TwitchUser_TwitchUserId",
                schema: "twitch-ai-client",
                table: "TextCompletion");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMessage_TwitchUser_TwitchUserId",
                schema: "twitch-ai-client",
                table: "UserMessage");

            migrationBuilder.DropIndex(
                name: "IX_UserMessage_TwitchUserId",
                schema: "twitch-ai-client",
                table: "UserMessage");

            migrationBuilder.DropIndex(
                name: "IX_TextCompletion_TwitchUserId",
                schema: "twitch-ai-client",
                table: "TextCompletion");

            migrationBuilder.DropColumn(
                name: "TwitchUserId",
                schema: "twitch-ai-client",
                table: "UserMessage");

            migrationBuilder.DropColumn(
                name: "TwitchUserId",
                schema: "twitch-ai-client",
                table: "TextCompletion");

            migrationBuilder.AlterColumn<Guid>(
                name: "TextCompletionId",
                schema: "twitch-ai-client",
                table: "Choice",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Choice_TextCompletion_TextCompletionId",
                schema: "twitch-ai-client",
                table: "Choice",
                column: "TextCompletionId",
                principalSchema: "twitch-ai-client",
                principalTable: "TextCompletion",
                principalColumn: "Id");
        }
    }
}
