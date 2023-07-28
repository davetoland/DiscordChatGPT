namespace DiscordChatGPT.Discord
{
    internal static class DiscordExtensions
    {
        internal static string GetDeferralUrl(this DiscordInteraction interaction) =>
            $"{DiscordConstants.ApiUrl}/interactions/{interaction.Id}/{interaction.Token}/callback";

        internal static string GetWebHookUrl(this DiscordInteraction interaction, string appId) =>
            $"{DiscordConstants.ApiUrl}/webhooks/{appId}/{interaction.Token}";

        internal static string GetCommandUrl(this string appId) =>
            $"{DiscordConstants.ApiUrl}/applications/{appId}/commands";

        internal static string FormatPromptResponse(this string prompt, string response) => $"{prompt}:\r{response}";
    }
}
