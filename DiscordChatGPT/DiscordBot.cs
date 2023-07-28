using DiscordChatGPT.Discord;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;

namespace DiscordChatGPT;

public class DiscordBot : IDisposable
{

    private readonly string? _appId;
    private readonly string? _token;
    private readonly HttpClient _client;
    private readonly ChatGptClient _chatGpt;

    public DiscordBot()
    {
        var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
        _appId = configuration?["discord:application-id"];
        _token = configuration?["discord:bot-token"];

        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bot {_token}");
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

        _chatGpt = new ChatGptClient(_client);
    }

    public async Task Run()
    {
        await RegisterCommand(new DiscordCommand("chat", 1, "Ask the AI Bot something", new[] {
            new DiscordCommandOption("text", 3, "Type your request for ChatGPT", true) 
        }));

        using var listener = new HttpListener();
        listener.Prefixes.Add("http://127.0.0.1:3000/");
        listener.Start();

        Console.WriteLine("Listening on port 3000...");

        while (true)
        {
            var context = listener.GetContext();
            Console.WriteLine($"Connection received from {context.Request.RemoteEndPoint}");

            var cancellationToken = CancellationToken.None;
            using var reader = new StreamReader(context.Request.InputStream);
            reader.Peek();

            var interaction = await JsonSerializer.DeserializeAsync<DiscordInteraction>(reader.BaseStream, DiscordConstants.JsonOptions, cancellationToken);
            if (interaction != null)
            {
                switch (interaction.Type)
                {
                    case DiscordConstants.Ping:
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        using (var writer = new StreamWriter(context.Response.OutputStream))
                        {
                            writer.Write(JsonSerializer.Serialize(new DiscordResponse(DiscordConstants.PingResponse)));
                            await writer.FlushAsync();
                        }
                        break;

                    case DiscordConstants.Callback:
                        await HandleCallback(interaction, cancellationToken);
                        break;
                }
            }
        }
    }

    private async Task RegisterCommand(DiscordCommand command)
    {
        if (string.IsNullOrWhiteSpace(_appId) || string.IsNullOrWhiteSpace(_token))
            throw new Exception("Failed to retrieve config from user-secrets");

        var url = _appId.GetCommandUrl();
        var response = await _client.PostAsync(url, JsonContent.Create(command));

        if (!response.IsSuccessStatusCode)
            Console.Error.WriteLine($"Failed to register command {command.Name}. {response.ReasonPhrase}");
        else
            Console.WriteLine($"Command '{command.Name}' added");
    }

    private async Task HandleCallback(DiscordInteraction? interaction, CancellationToken cancellationToken)
    {
        if (interaction == null)
            return;

        var deferUrl = interaction.GetDeferralUrl();
        var defer = new DiscordResponse(DiscordConstants.Deferral);
        var deferral = await _client.PostAsync(deferUrl, JsonContent.Create(defer), cancellationToken);
        if (deferral == null)
            Console.Error.WriteLine("Error sending deferral");

        var prompt = interaction.Data.Options.FirstOrDefault()?.Value;
        if (prompt == null || _appId == null)
            Console.Error.WriteLine("Error reading prompt");
        else
        {
            var chatGptResponse = await HandleChatGptInteraction(prompt, cancellationToken);

            var url = interaction.GetWebHookUrl(_appId);
            var update = prompt.FormatPromptResponse(chatGptResponse);
            var result = await _client.PostAsync(url, JsonContent.Create(update), cancellationToken);
            if (result == null)
                Console.Error.WriteLine("Error sending response");
            else if (!result.IsSuccessStatusCode)
            {
                var error = await result.Content.ReadAsStringAsync();
                Console.Error.WriteLine(error);
            }
        }
    }

    private async Task<string> HandleChatGptInteraction(string text, CancellationToken cancellationToken)
    {
        var payload = new Dictionary<string, object>
            {
                { "prompt", text },
                { "max_tokens", 1500 },
                { "model", "text-davinci-003" }
            };

        return await _chatGpt.GenerateText(payload, cancellationToken);
    }

    public void Dispose()
    {
        _client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
