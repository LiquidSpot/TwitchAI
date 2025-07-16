namespace TwitchAI.Application.Dto.Response
{
    public class TextCompletionDto
    {
        public string id { get; set; }

        public string @object { get; set; }

        public int created { get; set; }

        public string model { get; set; }

        public UsageDto? usage { get; set; }

        public virtual List<ChoiceDto>? choices { get; set; }
    }
}
