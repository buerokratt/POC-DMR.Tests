using Microsoft.Extensions.Configuration;
using Tests.IntegrationTests.Fixtures;
using Tests.IntegrationTests.Models;
using Tests.IntegrationTests.Extensions;
using Xunit.Abstractions;

namespace Tests.IntegrationTests
{
    public sealed class ClassifyMessageTests : IClassFixture<CentOpsFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly IConfiguration _configuration;
        private readonly TestClients _testClient;

        public ClassifyMessageTests(IConfiguration configuration, ITestOutputHelper output, TestClients testClients)
        {
            _configuration = configuration;
            _output = output;
            _testClient = testClients;
        }

        [Fact(Timeout = 2 * 60 * 1000)]
        public async Task EnvironmentIsConfigured()
        {
            // Arrange
            var institutionsUri = new Uri($"{_configuration["CentOpsUrl"]}/admin/institutions");
            var participantsUri = new Uri($"{_configuration["CentOpsUrl"]}/admin/participants");

            // Act
            var participants = await _testClient.CentOpsAdminClient.Request<List<Participant>>(Verb.Get, participantsUri).ConfigureAwait(false);
            var institutions = await _testClient.CentOpsAdminClient.Request<List<Institution>>(Verb.Get, institutionsUri).ConfigureAwait(false);

            // Assert
            _ = Assert.Single(institutions);
            Assert.Equal(3, participants.Count);
            Assert.Equal(participants[0].InstitutionId, institutions.Single().Id);
            Assert.Equal(participants[1].InstitutionId, institutions.Single().Id);
        }

        [Fact(Timeout = 4 * 60 * 1000)]
        public async Task GivenValidMessageReceivesValidResponse()
        {
            // Allow the environment to stabilise with CentOps participant configuration.
            await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

            _output.WriteLine($"Starting {nameof(GivenValidMessageReceivesValidResponse)}");

            // Arrange
            var chatsUri = new Uri($"{_configuration["Bot1Url"]}/client-api/chats");
            _output.WriteLine($"chatsUri = {chatsUri}");

            // Act
            // 1 Create Chat
            var createdChat = await _testClient.MockBotChatClient.Request<Chat>(Verb.Post, chatsUri, string.Empty).ConfigureAwait(false);
            _output.WriteLine($"createdChat.Id = {createdChat.Id}");

            // 2 Create Message
            var chatMessageUri = new Uri($"{_configuration["Bot1Url"]}/client-api/chats/{createdChat.Id}/messages");
            var createdChatMessage = await _testClient.MockBotChatClient.Request<ChatMessage>(Verb.Post, chatMessageUri, "i want to register my child at school").ConfigureAwait(false);
            _output.WriteLine($"createdChatMessage.Id = {createdChatMessage.Id}");

            // 3 Retry until we have 2 messages in the chat that this test created
            Chat resultChat = null;
            bool breakLoop = false;
            while (!breakLoop)
            {
                var chats = await _testClient.MockBotChatClient.Request<List<Chat>>(Verb.Get, chatsUri).ConfigureAwait(false);
                resultChat = chats.First(c => c.Id == createdChat.Id);
                _output.WriteLine($"resultChat.Messages.Count= {resultChat.Messages.Count}");

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
            _output.WriteLine($"dmrMessage.id = {dmrMessage.Id}");

            // Assert
            Assert.Equal(2, resultChat.Messages.Count);
            Assert.Equal("CLASSIFIER", dmrMessage.SentBy.ToUpperInvariant());
            Assert.Equal("EDUCATION", dmrMessage.Classification.ToUpperInvariant());
            Assert.Equal(createdChatMessage.Content, dmrMessage.Content);
        }
    }
}