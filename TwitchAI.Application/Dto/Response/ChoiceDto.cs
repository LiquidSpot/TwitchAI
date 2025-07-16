namespace TwitchAI.Application.Dto.Response
{
    public class ChoiceDto
    {
        public int? index { get; set; }

        public string? text { get; set; }

        public MessageDto? message { get; set; }

        public string? finish_reason { get; set; }
    }
}
