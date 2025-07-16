using TwitchAI.Domain.Entites.Abstract;

namespace TwitchAI.Domain.Entites
{
    /// <summary>
    /// usage
    /// </summary>
    public class Usage : Entity
    {
        public int prompt_tokens { get; set; }

        public int completion_tokens { get; set; }

        public int total_tokens { get; set; }
    };
}
