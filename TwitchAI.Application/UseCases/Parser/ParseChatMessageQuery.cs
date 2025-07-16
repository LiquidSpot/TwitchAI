using Common.Packages.Response.Behaviors;
using TwitchAI.Application.Interfaces;
using TwitchLib.Client.Models;

namespace TwitchAI.Application.UseCases.Parser;

public record ParseChatMessageQuery(ChatMessage RawMessage, Guid userId): IQuery<IChatCommand?>
{}