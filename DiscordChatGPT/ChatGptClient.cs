using DiscordChatGPT.ChatGpt;
using DiscordChatGPT.Responses;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace DiscordChatGPT;

public class ChatGptClient
{
    private readonly HttpClient _client;
    private readonly Uri _apiUrl;

    public ChatGptClient(HttpClient client)
    {
        // Retrieve the API key from the Secret Manager
        var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
        var apiKey = configuration["openai:api-key"] ?? throw new SystemException("Could not retrieve Api Key");

        _apiUrl = new Uri(ChatGptConstants.CompletionsUrl);
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<string> GenerateText(Dictionary<string, object> payload, CancellationToken cancellationToken)
    {
        // Format the payload
        var json = JsonSerializer.Serialize(payload);
        var data = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);

        // Send the API request and parse the response
        using var response = await _client.PostAsync(_apiUrl, data, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return response.ReasonPhrase ?? response.StatusCode.ToString();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = await JsonSerializer.DeserializeAsync<CompletionResponse>(stream, options, cancellationToken);
        return result?.GetFirstResponse() ?? "Error retrieving results from API";
    }
}
