namespace DiscordChatGPT.Responses
{
    public class CompletionResponse
    {
        public IReadOnlyList<Choice> Choices { get; set; } = new List<Choice>();

        public string Text => Choices.Count > 0 ? Choices[0].Text : "";
    }

    public class Choice
    {
        public string Text { get; set; } = string.Empty;
    }
}
