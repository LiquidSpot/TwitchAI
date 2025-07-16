using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwitchAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_GptEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageGpt_RequestModel_RequestModelId",
                schema: "twitch-ai-client",
                table: "MessageGpt");

            migrationBuilder.DropIndex(
                name: "IX_MessageGpt_RequestModelId",
                schema: "twitch-ai-client",
                table: "MessageGpt");

            migrationBuilder.DropColumn(
                name: "RequestModelId",
                schema: "twitch-ai-client",
                table: "MessageGpt");

            migrationBuilder.AddColumn<string>(
                name: "input",
                schema: "twitch-ai-client",
                table: "RequestModel",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "instructions",
                schema: "twitch-ai-client",
                table: "RequestModel",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "input",
                schema: "twitch-ai-client",
                table: "RequestModel");

            migrationBuilder.DropColumn(
                name: "instructions",
                schema: "twitch-ai-client",
                table: "RequestModel");

            migrationBuilder.AddColumn<Guid>(
                name: "RequestModelId",
                schema: "twitch-ai-client",
                table: "MessageGpt",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageGpt_RequestModelId",
                schema: "twitch-ai-client",
                table: "MessageGpt",
                column: "RequestModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageGpt_RequestModel_RequestModelId",
                schema: "twitch-ai-client",
                table: "MessageGpt",
                column: "RequestModelId",
                principalSchema: "twitch-ai-client",
                principalTable: "RequestModel",
                principalColumn: "Id");
        }
    }
}
