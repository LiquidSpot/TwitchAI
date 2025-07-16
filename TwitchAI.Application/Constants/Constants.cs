namespace TwitchAI.Application.Constants
{
    public static class Constants
    {
        public const string Scheme = "twitch-ai-client";
        public const string ConnectionString = "ConnectionString";
        public const string ServiceName = "Twtich AI Client";
        public const string OpenAiClientKey = "OpenAiClient";

        public static class OpenApiApis
        {
            // Новый Responses API (рекомендуется)
            public const string responses = "responses";
            
            // Старый Chat Completions API (для совместимости)
            public const string completions = "chat/completions";
        }
    }
}
