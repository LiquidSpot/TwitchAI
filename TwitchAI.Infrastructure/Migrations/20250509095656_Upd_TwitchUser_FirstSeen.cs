using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwitchAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Upd_TwitchUser_FirstSeen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstSeen",
                schema: "twitch-ai-client",
                table: "TwitchUser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FirstSeen",
                schema: "twitch-ai-client",
                table: "TwitchUser",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
