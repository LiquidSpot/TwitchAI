using System.Text.Json;

using TwitchAI.Application.Dto.Response;

namespace TwitchAI.Application.Behaviors.MapperProfiles
{
    public static class MappingHelpers
    {
        public static string? SerializeMessage(MessageDto? m) =>
            m == null ? null : JsonSerializer.Serialize(m);
    }
}
