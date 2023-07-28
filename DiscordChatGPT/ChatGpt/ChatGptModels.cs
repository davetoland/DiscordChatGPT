namespace DiscordChatGPT.Responses
{
    internal record CompletionResponse(IReadOnlyList<Choice> Choices);

    internal record Choice(string Text);
}
