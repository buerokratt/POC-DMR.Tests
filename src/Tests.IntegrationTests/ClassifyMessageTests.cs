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
            var chatsResponse = await client.PostAsync(chatsUri, null).ConfigureAwait(false);
            _ = chatsResponse.EnsureSuccessStatusCode();
            var chatsResponseContent = await chatsResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            var chat = JsonSerializer.Deserialize<Chat>(chatsResponseContent);

            // 2 Create Message
            var chatMessageUri = new Uri($"{bot1Url}/client-api/chats/{chat.Id}/messages");
            using var content = new StringContent("i want to register my child at school");
            var chatMessageResponse = await client.PostAsync(chatMessageUri, content).ConfigureAwait(false);
            _ = chatMessageResponse.EnsureSuccessStatusCode();
            var chatMessageResponseContent = await chatMessageResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            var chatMessage = JsonSerializer.Deserialize<ChatMessage>(chatMessageResponseContent);


            // Assert
            Assert.Equal("http://localhost:9012", bot1Url.ToString());
        }
    }
}