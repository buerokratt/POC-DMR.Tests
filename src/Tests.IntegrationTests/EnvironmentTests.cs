using Microsoft.Extensions.Configuration;
using Tests.IntegrationTests.Fixtures;
using Tests.IntegrationTests.Helpers;
using Tests.IntegrationTests.Models;

namespace Tests.IntegrationTests
{
    public class EnvironmentTests : IClassFixture<CentOpsFixture>
    {
        private readonly IConfiguration _configuration;
        private readonly Uri _institutionsUri;
        private readonly Uri _participantsUri;

        public EnvironmentTests(IConfiguration configuration)
        {
            _configuration = configuration;
            _institutionsUri = new Uri($"{_configuration["CentOpsUrl"]}/admin/institutions");
            _participantsUri = new Uri($"{_configuration["CentOpsUrl"]}/admin/participants");
        }

        [Fact(Timeout = 120000)]
        public async Task EnvironmentIsConfigured()
        {
            // Arrange
            using var httpClient = new HttpClient();

            // Act
            var participants = await RequestHelper.Request<List<Participant>>(httpClient, Verb.Get, _participantsUri, _configuration["CentOpsApiKey"]).ConfigureAwait(false);
            var institutions = await RequestHelper.Request<List<Institution>>(httpClient, Verb.Get, _institutionsUri, _configuration["CentOpsApiKey"]).ConfigureAwait(false);

            // Assert
            _ = Assert.Single(institutions);
            var institutionId = institutions.FirstOrDefault().Id;
            Assert.Equal(2, participants.Count);
            Assert.Equal(participants[0].InstitutionId, institutionId);
            Assert.Equal(participants[1].InstitutionId, institutionId);
        }
    }
}

