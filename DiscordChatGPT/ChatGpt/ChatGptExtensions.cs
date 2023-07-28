namespace DiscordChatGPT.Responses
{
    internal static class ChatGptExtensions
    {
        internal static string? GetFirstResponse(this CompletionResponse response) => 
            response.Choices.Count > 0 ? response.Choices[0].Text : null;
    }
}
