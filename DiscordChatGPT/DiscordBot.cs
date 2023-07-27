using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;

namespace DiscordChatGPT
{
    public class DiscordBot : IDisposable
    {
        private const string _discordApi = "https://discord.com/api/v10";
        private const string _openAiApi = "https://api.openai.com/v1/completions";

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
            _chatGpt = new ChatGptClient(new Uri(_openAiApi), _client);
        }

        public async Task Run()
        {
            await RegisterCommands();

            using var listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:3000/");
            listener.Start();

            Console.WriteLine("Listening on port 3000...");

            while (true)
            {
                var context = listener.GetContext();
                Console.WriteLine($"Connection received from {context.Request.RemoteEndPoint}");

                using var writer = new StreamWriter(context.Response.OutputStream);
                using var reader = new StreamReader(context.Request.InputStream);
                var input = await reader.ReadToEndAsync();
                var interaction = JsonConvert.DeserializeAnonymousType(input,
                    new
                    {
                        id = "",
                        token = "",
                        type = 0,
                        data = new
                        {
                            name = "",
                            options = new[] { new { value = "" } }
                        }
                    });

                if (interaction != null)
                {
                    switch (interaction.type)
                    {
                        case 1:
                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                            writer.Write(JsonConvert.SerializeObject(new { type = 1 }));
                            await writer.FlushAsync();
                            break;

                        case 2:
                            var deferUrl = $"{_discordApi}/interactions/{interaction.id}/{interaction.token}/callback";
                            var defer = new { type = 5 };
                            var deferral = await _client.PostAsync(deferUrl, JsonContent.Create(defer));
                            if (deferral == null)
                                Console.Error.WriteLine("Error sending deferral");

                            var chatGptResponse = await HandleChatGptInteraction(interaction.data.options[0].value);

                            var url = $"{_discordApi}/webhooks/{_appId}/{interaction.token}";
                            var update = new { content = interaction.data.options[0].value + "...\r" + chatGptResponse };
                            var result = await _client.PostAsync(url, JsonContent.Create(update));
                            if (result == null)
                                Console.Error.WriteLine("Error sending response");
                            else if (!result.IsSuccessStatusCode)
                            {
                                var error = await result.Content.ReadAsStringAsync();
                                Console.Error.WriteLine(error);
                            }
                            break;
                    }
                }
            }
        }

        private async Task RegisterCommands()
        {
            if (string.IsNullOrWhiteSpace(_appId) || string.IsNullOrWhiteSpace(_token))
                throw new Exception("Failed to retrieve config from user-secrets");

            var url = $"{_discordApi}/applications/{_appId}/commands";
            var commands = new[]
            {
                new
                {
                    name = "chat",
                    type = 1,
                    description = "Ask the AI Bot something",
                    options = new dynamic[]
                    {
                        new
                        {
                            name = "text",
                            description = "Type your request for ChatGPT",
                            type = 3,
                            required = true
                        }
                    }
                }
            };

            foreach (var command in commands)
            {
                var response = await _client.PostAsync(url, JsonContent.Create(command));
                if (!response.IsSuccessStatusCode)
                    Console.Error.WriteLine($"Failed to register command {command.name}. {response.ReasonPhrase}");
                else
                    Console.WriteLine($"Command '{command.name}' added");
            }
        }

        private async Task<string> HandleChatGptInteraction(string text)
        {
            var payload = new Dictionary<string, object>
            {
                { "prompt", text },
                { "max_tokens", 1500 },
                { "model", "text-davinci-003" }
            };

            return await _chatGpt.GenerateText(payload);
        }

        public void Dispose()
        {
            _client?.Dispose();
            _chatGpt?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
