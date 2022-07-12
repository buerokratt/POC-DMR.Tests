using Microsoft.Extensions.Configuration;
using Tests.IntegrationTests.Fixtures;
using Tests.IntegrationTests.Helpers;
using Tests.IntegrationTests.Models;

namespace Tests.IntegrationTests
{
    public sealed class ClassifyMessageTests : IClassFixture<CentOpsFixture>, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;

        public ClassifyMessageTests(IConfiguration configuration)
        {
            _configuration = configuration;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("x-api-key", _configuration["CentOpsApiKey"]);

        }

        [Fact(Timeout = 120000)]
        public async Task EnvironmentIsConfigured()
        {
            // Arrange
            var institutionsUri = new Uri($"{_configuration["CentOpsUrl"]}/admin/institutions");
            var participantsUri = new Uri($"{_configuration["CentOpsUrl"]}/admin/participants");

            // Act
            var participants = await RequestHelper.Request<List<Participant>>(_client, Verb.Get, participantsUri).ConfigureAwait(false);
            var institutions = await RequestHelper.Request<List<Institution>>(_client, Verb.Get, institutionsUri).ConfigureAwait(false);

            // Assert
            _ = Assert.Single(institutions);
            Assert.Equal(2, participants.Count);
            Assert.Equal(participants[0].InstitutionId, institutions.Single().Id);
            Assert.Equal(participants[1].InstitutionId, institutions.Single().Id);
        }

        [Fact(Timeout = 120000)]
        public async Task GivenValidMessageReceivesValidResponse()
        {
            // Arrange
            var chatsUri = new Uri($"{_configuration["Bot1Url"]}/client-api/chats");

            // Act
            // 1 Create Chat
            var createdChat = await RequestHelper.Request<Chat>(_client, Verb.Post, chatsUri, _configuration["CentOpsApiKey"]).ConfigureAwait(false);

            // 2 Create Message
            var chatMessageUri = new Uri($"{_configuration["Bot1Url"]}/client-api/chats/{createdChat.Id}/messages");
            var createdChatMessage = await RequestHelper.Request<ChatMessage>(_client, Verb.Post, chatMessageUri, "i want to register my child at school").ConfigureAwait(false);

            // 3 Retry until we have 2 messages in the chat that this test created
            Chat resultChat = null;
            bool breakLoop = false;
            while (!breakLoop)
            {
                var chats = await RequestHelper.Request<List<Chat>>(_client, Verb.Get, chatsUri).ConfigureAwait(false);
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

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}