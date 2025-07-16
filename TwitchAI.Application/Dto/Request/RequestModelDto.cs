namespace TwitchAI.Application.Dto.Request
{
    public class RequestModelDto
    {
        public string model { get; set; } = "gpt-4o";

        public int? max_tokens { get; set; } = 700;

        public double? temperature { get; set; } = 0.8;

        public MessageGptDto[] messages { get; set; }
    }
}
