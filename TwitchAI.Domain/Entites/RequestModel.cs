using TwitchAI.Domain.Entites.Abstract;

namespace TwitchAI.Domain.Entites
{
    public class RequestModel : Entity
    {
        public string model { get; set; } = "gpt-4o";

        public int? max_tokens { get; set; } = 700;

        public double? temperature { get; set; } = 0.8;

        public MessageGpt[] messages { get; set; }
    }
}
