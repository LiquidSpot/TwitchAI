namespace TwitchAI.Application.Models;

public class TwitchConfiguration
{
    public string ChannelName{ get; set; } = "your_twitch_channel";
    public string BotUsername { get; set; } = "your_bot_username";
    public string BotAccessToken { get; set; } = "your_bot_access_token";
    public string BotRefreshToken { get; set; } = "your_bot_refresh_token";
    public string ClientId { get; set; } = "your_twitch_client_id";
    public string IrcServer { get; set; } = "irc.chat.twitch.tv";
}