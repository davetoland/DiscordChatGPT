using System.Text.Json;

namespace DiscordChatGPT.Discord
{
    internal class DiscordConstants
    {
        internal const int Ping = 1;
        internal const int PingResponse = 1;
        internal const int Callback = 2;
        internal const int Deferral = 5;

        internal const string ApiUrl = "https://discord.com/api/v10";

        internal static JsonSerializerOptions JsonOptions => new() { PropertyNameCaseInsensitive = true };
    }
}
