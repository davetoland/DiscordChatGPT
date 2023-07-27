using DiscordChatGPT.Responses;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

namespace DiscordChatGPT
{
    public class ChatGptClient : IDisposable
    {
        private readonly HttpClient _client;
        private readonly Uri _apiUrl;

        public ChatGptClient(Uri apiUrl, HttpClient client)
        {
            _apiUrl = apiUrl;
            _client = client;

            // Retrieve the API key from the Secret Manager
            var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
            var apiKey = configuration["openai:api-key"] ?? throw new SystemException("Could not retrieve Api Key");

            // Set headers for each request
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        }

        public async Task<string> GenerateText(Dictionary<string, object> payload)
        {
            // Format the payload
            var json = JsonConvert.SerializeObject(payload);
            var data = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);

            // Send the API request and parse the response
            var response = await _client.PostAsync(_apiUrl, data);
            if (!response.IsSuccessStatusCode)
                return response.ReasonPhrase ?? response.StatusCode.ToString();

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<CompletionResponse>(responseContent);
            if (result == null)
                return "Error retrieving results from API";

            return result.Text;
        }

        public void Dispose()
        {
            _client?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
