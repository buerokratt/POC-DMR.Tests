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

        [Fact(Timeout = 120000)]
        public async Task GivenValidMessageReceivesValidResponse()
        {
            // Arrange
            using var httpClient = new HttpClient();
            var bot1Url = _configuration["Bot1Url"];
            var chatsUri = new Uri($"{bot1Url}/client-api/chats");

            // Act
            // 1 Create Chat
            var createdChat = await Request<Chat>(httpClient, Verb.Post, chatsUri).ConfigureAwait(false);

            // 2 Create Message
            var chatMessageUri = new Uri($"{bot1Url}/client-api/chats/{createdChat.Id}/messages");
            using var content = new StringContent("i want to register my child at school");
            var createdChatMessage = await Request<ChatMessage>(httpClient, Verb.Post, chatMessageUri, content).ConfigureAwait(false);

            // 3 Retry until we have 2 messages in the chat that this test created
            Chat resultChat = null;
            bool breakLoop = false;
            while (!breakLoop)
            {
                var chats = await Request<List<Chat>>(httpClient, Verb.Get, chatsUri).ConfigureAwait(false);
                resultChat = chats.First(c => c.Id == createdChat.Id);
                if (resultChat.Messages.Count < 2)
                {
                    await Task.Delay(new TimeSpan(0, 0, 10)).ConfigureAwait(false);
                }
                else
                {
                    breakLoop = true;
                }
            }

            // Determine the newest message (assume this is from Dmr)
            var dmrMessage = resultChat.Messages.OrderByDescending(c => c.CreatedAt).First();

            // Assert
            Assert.Equal(2, resultChat.Messages.Count);
            Assert.Equal("CLASSIFIER", dmrMessage.SentBy.ToUpperInvariant());
            Assert.Equal("EDUCATION", dmrMessage.Classification.ToUpperInvariant());
            Assert.Equal(createdChatMessage.Content, dmrMessage.Content);
        }

        /// <summary>
        /// Simple helper to handle http requests and deserialisation of result
        /// </summary>
        /// <typeparam name="T">Type that the result shoudl be deserialised to</typeparam>
        /// <param name="httpClient">A HttpClient to use/reuse</param>
        /// <param name="verb">Which Http verb to use</param>
        /// <param name="uri">The Uri to send the request to</param>
        /// <param name="body">An optional body to send with the request with Post requests</param>
        /// <returns>Object representing deserialised result of type defined by T</returns>
        /// <exception cref="NotImplementedException">If verb is not in expected range.</exception>
        private static async Task<T> Request<T>(HttpClient httpClient, Verb verb, Uri uri, StringContent body = null)
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