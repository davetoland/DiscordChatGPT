namespace DiscordChatGPT.Discord
{
    internal record DiscordCommand(string Name, int Type, string Description, IEnumerable<DiscordCommandOption> Options);

    internal record DiscordCommandOption(string Name, int Type, string Description, bool Required);
    internal record DiscordInteraction(string Id, string Token, int Type, DiscordData Data);

    internal record DiscordData(string Name, IEnumerable<DiscordOption> Options);

    internal record DiscordOption(string Value);

    internal record DiscordResponse(int Type);

    internal record DiscordContent(string Content);
}
