using Microsoft.Extensions.Configuration;
using Tests.IntegrationTests.Fixtures;
using Tests.IntegrationTests.Models;
using Tests.IntegrationTests.Extensions;
using Xunit.Abstractions;

namespace Tests.IntegrationTests
{
    public sealed class ClassifyMessageTests : IClassFixture<CentOpsFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;

        public ClassifyMessageTests(IConfiguration configuration, ITestOutputHelper output)
        {
            _configuration = configuration;
            _output = output;
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
            var participants = await _client.Request<List<Participant>>(Verb.Get, participantsUri).ConfigureAwait(false);
            var institutions = await _client.Request<List<Institution>>(Verb.Get, institutionsUri).ConfigureAwait(false);

            // Assert
            _ = Assert.Single(institutions);
            Assert.Equal(2, participants.Count);
            Assert.Equal(participants[0].InstitutionId, institutions.Single().Id);
            Assert.Equal(participants[1].InstitutionId, institutions.Single().Id);
        }

        [Fact(Timeout = 120000)]
        public async Task GivenValidMessageReceivesValidResponse()
        {
            _output.WriteLine("Starting GivenValidMessageReceivesValidResponse");

            // Arrange
            var chatsUri = new Uri($"{_configuration["Bot1Url"]}/client-api/chats");
            _output.WriteLine($"chatsuri = {chatsUri}");

            // Act
            // 1 Create Chat
            var createdChat = await _client.Request<Chat>(Verb.Post, chatsUri, _configuration["CentOpsApiKey"]).ConfigureAwait(false);

            // 2 Create Message
            var chatMessageUri = new Uri($"{_configuration["Bot1Url"]}/client-api/chats/{createdChat.Id}/messages");
            var createdChatMessage = await _client.Request<ChatMessage>(Verb.Post, chatMessageUri, "i want to register my child at school").ConfigureAwait(false);

            // 3 Retry until we have 2 messages in the chat that this test created
            Chat resultChat = null;
            bool breakLoop = false;
            while (!breakLoop)
            {
                var chats = await _client.Request<List<Chat>>(Verb.Get, chatsUri).ConfigureAwait(false);
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


            _output.WriteLine($"resultChat = {resultChat}");

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