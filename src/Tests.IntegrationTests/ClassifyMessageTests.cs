using Microsoft.Extensions.Configuration;
using Tests.IntegrationTests.Fixtures;
using Tests.IntegrationTests.Helpers;
using Tests.IntegrationTests.Models;

namespace Tests.IntegrationTests
{
    public class ClassifyMessageTests : IClassFixture<CentOpsFixture>
    {
        private readonly IConfiguration _configuration;

        public ClassifyMessageTests(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Fact(Timeout = 120000)]
        public async Task GivenValidMessageReceivesValidResponse()
        {
            // Arrange
            using var httpClient = new HttpClient();
            var chatsUri = new Uri($"{_configuration["Bot1Url"]}/client-api/chats");

            // Act
            // 1 Create Chat
            var createdChat = await RequestHelper.Request<Chat>(httpClient, Verb.Post, chatsUri, _configuration["CentOpsApiKey"]).ConfigureAwait(false);

            // 2 Create Message
            var chatMessageUri = new Uri($"{_configuration["Bot1Url"]}/client-api/chats/{createdChat.Id}/messages");
            var createdChatMessage = await RequestHelper.Request<ChatMessage>(httpClient, Verb.Post, chatMessageUri, _configuration["CentOpsApiKey"], "i want to register my child at school").ConfigureAwait(false);

            // 3 Retry until we have 2 messages in the chat that this test created
            Chat resultChat = null;
            bool breakLoop = false;
            while (!breakLoop)
            {
                var chats = await RequestHelper.Request<List<Chat>>(httpClient, Verb.Get, chatsUri, _configuration["CentOpsApiKey"]).ConfigureAwait(false);
                resultChat = chats.First(c => c.Id == createdChat.Id);
                if (resultChat.Messages.Count < 2)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
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
    }
}