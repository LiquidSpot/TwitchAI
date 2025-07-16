using Common.Packages.Response.Behaviors;
using Common.Packages.Response.Models;

using TwitchAI.Application.Dto.Response;
using TwitchLib.Client.Models;

namespace TwitchAI.Application.UseCases.Twitch.Message;

public record HandleMessageCommand(ChatMessage message) : ICommand<LSResponse<ChatMessageDto>>;