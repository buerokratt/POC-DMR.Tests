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
            using var client = new HttpClient();

            // Act
            // 1 Create Chat
            var createChatResponse = await client.PostAsync(chatsUri, null).ConfigureAwait(false);
            _ = createChatResponse.EnsureSuccessStatusCode();
            var createChatResponseContent = await createChatResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            var createdChat = JsonSerializer.Deserialize<Chat>(createChatResponseContent);

            // 2 Create Message
            var chatMessageUri = new Uri($"{bot1Url}/client-api/chats/{createdChat.Id}/messages");
            using var content = new StringContent("i want to register my child at school");
            var chatMessageResponse = await client.PostAsync(chatMessageUri, content).ConfigureAwait(false);
            _ = chatMessageResponse.EnsureSuccessStatusCode();
            var chatMessageResponseContent = await chatMessageResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            var chatMessage = JsonSerializer.Deserialize<ChatMessage>(chatMessageResponseContent);

            // 3 Loop or delay
            Thread.Sleep(new TimeSpan(0, 0, 15)); // Temporary, will add proper retry loop with expanding backoff up to a timeout.

            // 4 Get all chats
            var chatsResponse = await client.GetAsync(chatsUri).ConfigureAwait(false);
            _ = chatsResponse.EnsureSuccessStatusCode();
            var chatsResponseContent = await chatsResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            var chats = JsonSerializer.Deserialize<List<Chat>>(chatsResponseContent);

            // 5 Identify this test's chat
            var testResultChat = chats.FirstOrDefault(c => c.Id == createdChat.Id);

            // Assert
            Assert.Equal(2, testResultChat.Messages.Count);
        }
    }
}