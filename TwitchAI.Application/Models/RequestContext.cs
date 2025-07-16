namespace TwitchAI.Application.Models
{
    public static class RequestContext
    {
        private static readonly AsyncLocal<string?> _id = new();
        public static string? Id
        {
            get => _id.Value;
            set => _id.Value = value;
        }
    }
}
