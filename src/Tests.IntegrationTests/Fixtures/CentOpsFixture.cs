using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Tests.IntegrationTests.Helpers;
using Tests.IntegrationTests.Models;

namespace Tests.IntegrationTests.Fixtures
{
    public sealed class CentOpsFixture : IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly string _testId;

        public CentOpsFixture(IConfiguration configuration)
        {
            // Do "global" initialization here; Only called once.

            // Setup
            _configuration = configuration;
            _testId = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.CurrentCulture);
            using var httpClient = new HttpClient();
            var institutionsUri = new Uri($"{_configuration["CentOpsUrl"]}/admin/institutions");
            var participantsUri = new Uri($"{_configuration["CentOpsUrl"]}/admin/participants");

            // Create Institution
            var institutionPostBody = JsonSerializer.Serialize(new InstitutionPost()
            {
                Name = $"TestInstitution{_testId}",
            });
            var institution = RequestHelper.Request<Institution>(httpClient, Verb.Post, institutionsUri, _configuration["CentOpsApiKey"], institutionPostBody).Result;

            // Create Dmr
            var dmrPostBody = JsonSerializer.Serialize(new ParticipantPost()
            {
                Name = "dmr1",
                InstitutionId = institution.Id,
                Host = "http://dmr/messages",
                Type = "Dmr",
                Status = "Active",
                ApiKey = "thisisareallylongkey"
            });
            var dmr = RequestHelper.Request<ParticipantPost>(httpClient, Verb.Post, participantsUri, _configuration["CentOpsApiKey"], dmrPostBody).Result;

            // Create Bot1
            var bot1PostBody = JsonSerializer.Serialize(new ParticipantPost()
            {
                Name = "bot1",
                InstitutionId = institution.Id,
                Host = "http://bot1/dmr-api/messages",
                Type = "Chatbot",
                Status = "Active",
                ApiKey = "thisisareallylongkey"
            });
            var bot1 = RequestHelper.Request<ParticipantPost>(httpClient, Verb.Post, participantsUri, _configuration["CentOpsApiKey"], bot1PostBody).Result;
        }

        public void Dispose()
        {
            // Do "global" teardown here; Only called once.
        }
    }
}

