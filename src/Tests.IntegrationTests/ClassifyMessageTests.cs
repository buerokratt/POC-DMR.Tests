using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Tests.IntegrationTests.Models;

namespace Tests.IntegrationTests
{
    public class ClassifyMessageTests
    {
        private readonly IConfiguration _configuration;

        public ClassifyMessageTests()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile(@"appsettings.json", false, false)
                .Build();
        }

        [Fact]
        public async Task GivenValidMessageReceivesValidResponse()
        {
            // Arrange
            var bot1Url = _configuration["Bot1Url"];
            var chatsUri = new Uri($"{bot1Url}/client-api/chats");

            // Act
            // 1 Create Chat
            var createChatResponseContent = await Post(chatsUri).ConfigureAwait(false);
            var createdChat = JsonSerializer.Deserialize<Chat>(createChatResponseContent);

            // 2 Create Message
            var chatMessageUri = new Uri($"{bot1Url}/client-api/chats/{createdChat.Id}/messages");
            using var content = new StringContent("i want to register my child at school");
            var chatMessageResponseContent = await Post(chatMessageUri, content).ConfigureAwait(false);
            var chatMessage = JsonSerializer.Deserialize<ChatMessage>(chatMessageResponseContent);

            // 3 Loop or delay
            Thread.Sleep(new TimeSpan(0, 0, 15)); // Temporary, will add proper retry loop with expanding backoff up to a timeout.

            // 4 Get all chats
            using var httpClient = new HttpClient();
            var chatsResponse = await httpClient.GetAsync(chatsUri).ConfigureAwait(false);
            _ = chatsResponse.EnsureSuccessStatusCode();
            var chatsResponseContent = await chatsResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            var chats = JsonSerializer.Deserialize<List<Chat>>(chatsResponseContent);

            // 5 Identify this test's chat
            var testResultChat = chats.FirstOrDefault(c => c.Id == createdChat.Id);

            // Assert
            Assert.Equal(2, testResultChat.Messages.Count);
        }

        private static async Task<string> Post(Uri uri, StringContent body = null)
        {
            // TO DO Add verb param and make it work for all requests
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(uri, body).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return content;
        }
    }
}