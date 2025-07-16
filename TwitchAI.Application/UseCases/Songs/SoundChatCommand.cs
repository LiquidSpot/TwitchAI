using Common.Packages.Response.Behaviors;
using TwitchAI.Application.Interfaces;

namespace TwitchAI.Application.UseCases.Songs
{
    public record SoundChatCommand(string RawMessage): IChatCommand;
}
