namespace TwitchAI.Application.Dto.Response
{
    public class MessageDto
    {
        public string role { get; init; } = default!;
        public string content { get; init; } = default!;
        public string? refusal { get; init; }
        public object? annotations { get; init; }
    }
}
