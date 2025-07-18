namespace TwitchAI.Application.Constants
{
    public static class Constants
    {
        public const string Scheme = "twitch-ai-client";
        public const string ConnectionString = "ConnectionString";
        public const string ServiceName = "Twtich AI Client";
        public const string OpenAiClientKey = "OpenAiClient";
        public const string TwitchApiClientKey = "TwitchApiClient";
        public const string OpenHolidaysApiClientKey = "OpenHolidaysApiClient";

        public static class OpenApiApis
        {
            // Новый Responses API (рекомендуется)
            public const string responses = "responses";
            
            // Старый Chat Completions API (для совместимости)
            public const string completions = "chat/completions";
        }

        public static class TwitchApis
        {
            public const string BaseUrl = "https://api.twitch.tv/helix";
            public const string GetChatters = "chat/chatters";
            public const string GetStreams = "streams";
            public const string GetUsers = "users";
            public const string GetChannelInformation = "channels";
        }

        public static class OpenHolidaysApis
        {
            public const string BaseUrl = "https://openholidaysapi.org";
            public const string GetPublicHolidays = "PublicHolidays";
            public const string GetCountries = "Countries";
            public const string GetLanguages = "Languages";
            public const string GetSubdivisions = "Subdivisions";
        }
    }
}
