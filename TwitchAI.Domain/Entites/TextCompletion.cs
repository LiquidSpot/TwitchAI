using TwitchAI.Domain.Entites.Abstract;

namespace TwitchAI.Domain.Entites
{
    public class TextCompletion : Entity
    {
        public string GptId{ get; set; }

        public string @object { get; set; }

        public int created { get; set; }

        public string model { get; set; }

        public int IdUsage { get; set; }

        public Usage? Usage { get; set; }


        // Navigation properties
        public Guid TwitchUserId { get; set; }
        public virtual TwitchUser? TwitchUser { get; set; }

        public virtual List<Choice>? Choices { get; set; }
    }
}
