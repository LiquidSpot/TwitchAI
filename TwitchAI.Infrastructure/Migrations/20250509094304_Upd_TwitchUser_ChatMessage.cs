using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwitchAI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Upd_TwitchUser_ChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Message",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                newName: "TmiSentTs");

            migrationBuilder.AddColumn<string>(
                name: "BadgesJson",
                schema: "twitch-ai-client",
                table: "TwitchUser",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CheerBits",
                schema: "twitch-ai-client",
                table: "TwitchUser",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorHex",
                schema: "twitch-ai-client",
                table: "TwitchUser",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                schema: "twitch-ai-client",
                table: "TwitchUser",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstSeen",
                schema: "twitch-ai-client",
                table: "TwitchUser",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsBroadcaster",
                schema: "twitch-ai-client",
                table: "TwitchUser",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPartner",
                schema: "twitch-ai-client",
                table: "TwitchUser",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsStaff",
                schema: "twitch-ai-client",
                table: "TwitchUser",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTurbo",
                schema: "twitch-ai-client",
                table: "TwitchUser",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVip",
                schema: "twitch-ai-client",
                table: "TwitchUser",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeen",
                schema: "twitch-ai-client",
                table: "TwitchUser",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "SubscribedMonthCount",
                schema: "twitch-ai-client",
                table: "TwitchUser",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GptId",
                schema: "twitch-ai-client",
                table: "TextCompletion",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Bits",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BitsUsd",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Channel",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmoteReplacedText",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBroadcaster",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFirstMessage",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHighlighted",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMeAction",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsModerator",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSkippingSubMode",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubscriber",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTurbo",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MessageId",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RawTagsJson",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RoomId",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Text",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BadgesJson",
                schema: "twitch-ai-client",
                table: "TwitchUser");

            migrationBuilder.DropColumn(
                name: "CheerBits",
                schema: "twitch-ai-client",
                table: "TwitchUser");

            migrationBuilder.DropColumn(
                name: "ColorHex",
                schema: "twitch-ai-client",
                table: "TwitchUser");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                schema: "twitch-ai-client",
                table: "TwitchUser");

            migrationBuilder.DropColumn(
                name: "FirstSeen",
                schema: "twitch-ai-client",
                table: "TwitchUser");

            migrationBuilder.DropColumn(
                name: "IsBroadcaster",
                schema: "twitch-ai-client",
                table: "TwitchUser");

            migrationBuilder.DropColumn(
                name: "IsPartner",
                schema: "twitch-ai-client",
                table: "TwitchUser");

            migrationBuilder.DropColumn(
                name: "IsStaff",
                schema: "twitch-ai-client",
                table: "TwitchUser");

            migrationBuilder.DropColumn(
                name: "IsTurbo",
                schema: "twitch-ai-client",
                table: "TwitchUser");

            migrationBuilder.DropColumn(
                name: "IsVip",
                schema: "twitch-ai-client",
                table: "TwitchUser");

            migrationBuilder.DropColumn(
                name: "LastSeen",
                schema: "twitch-ai-client",
                table: "TwitchUser");

            migrationBuilder.DropColumn(
                name: "SubscribedMonthCount",
                schema: "twitch-ai-client",
                table: "TwitchUser");

            migrationBuilder.DropColumn(
                name: "GptId",
                schema: "twitch-ai-client",
                table: "TextCompletion");

            migrationBuilder.DropColumn(
                name: "Bits",
                schema: "twitch-ai-client",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "BitsUsd",
                schema: "twitch-ai-client",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "Channel",
                schema: "twitch-ai-client",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "EmoteReplacedText",
                schema: "twitch-ai-client",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "IsBroadcaster",
                schema: "twitch-ai-client",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "IsFirstMessage",
                schema: "twitch-ai-client",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "IsHighlighted",
                schema: "twitch-ai-client",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "IsMeAction",
                schema: "twitch-ai-client",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "IsModerator",
                schema: "twitch-ai-client",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "IsSkippingSubMode",
                schema: "twitch-ai-client",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "IsSubscriber",
                schema: "twitch-ai-client",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "IsTurbo",
                schema: "twitch-ai-client",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "MessageId",
                schema: "twitch-ai-client",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "RawTagsJson",
                schema: "twitch-ai-client",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "RoomId",
                schema: "twitch-ai-client",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "Text",
                schema: "twitch-ai-client",
                table: "ChatMessage");

            migrationBuilder.RenameColumn(
                name: "TmiSentTs",
                schema: "twitch-ai-client",
                table: "ChatMessage",
                newName: "Message");
        }
    }
}
