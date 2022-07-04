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
            using var httpClient = new HttpClient();
            var bot1Url = _configuration["Bot1Url"];
            var chatsUri = new Uri($"{bot1Url}/client-api/chats");

            // Act
            // 1 Create Chat
            var createdChat = await Request<Chat>(httpClient, Verb.Post, chatsUri).ConfigureAwait(false) as Chat;

            // 2 Create Message
            var chatMessageUri = new Uri($"{bot1Url}/client-api/chats/{createdChat.Id}/messages");
            using var content = new StringContent("i want to register my child at school");
            _ = await Request<ChatMessage>(httpClient, Verb.Post, chatMessageUri, content).ConfigureAwait(false);

            // 3 Loop or delay
            Thread.Sleep(new TimeSpan(0, 0, 15)); // Temporary, will add proper retry loop with expanding backoff up to a timeout.

            // 4 Get all chats
            var chats = await Request<List<Chat>>(httpClient, Verb.Get, chatsUri).ConfigureAwait(false) as List<Chat>;

            // 5 Identify this test's chat
            var testResultChat = chats.FirstOrDefault(c => c.Id == createdChat.Id);

            // Assert
            Assert.Equal(2, testResultChat.Messages.Count);
        }

        private static async Task<object> Request<T>(HttpClient httpClient, Verb verb, Uri uri, StringContent body = null)
        {
            var response = verb switch
            {
                Verb.Post => await httpClient.PostAsync(uri, body).ConfigureAwait(false),
                Verb.Get => await httpClient.GetAsync(uri).ConfigureAwait(false),
                _ => throw new NotImplementedException(),
            };
            _ = response.EnsureSuccessStatusCode();
            var contentRaw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<T>(contentRaw);
            return result;
        }
    }
}