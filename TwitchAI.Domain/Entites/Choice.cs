using TwitchAI.Domain.Entites.Abstract;

namespace TwitchAI.Domain.Entites
{
    public class Choice : Entity
    {
        public int? Index { get; set; }

        public string? Text { get; set; }

        public string? Message{ get; set; }

        public string? FinishReason { get; set; }

        // Navigation properties
        public Guid TextCompletionId { get; set; }
        public virtual TextCompletion? TextCompletion { get; set; }
    }
}
