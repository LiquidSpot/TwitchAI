using TwitchAI.Application.Interfaces;

namespace TwitchAI.Application.UseCases.OpenAi;

public record EngineCommand(string EngineName, Guid UserId) : IChatCommand;