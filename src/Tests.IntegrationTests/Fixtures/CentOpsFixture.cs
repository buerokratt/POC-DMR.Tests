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
        private readonly Uri _institutionsUri;
        private readonly Uri _participantsUri;

        public CentOpsFixture(IConfiguration configuration)
        {
            // Do "global" initialization here; Only called once.

            // Setup
            _configuration = configuration;
            _testId = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.CurrentCulture);
            using var httpClient = new HttpClient();
            _institutionsUri = new Uri($"{_configuration["CentOpsUrl"]}/admin/institutions");
            _participantsUri = new Uri($"{_configuration["CentOpsUrl"]}/admin/participants");

            // Create Institution
            var institutionPostBody = JsonSerializer.Serialize(new InstitutionPost()
            {
                Name = $"TestInstitution{_testId}",
            });
            var institution = RequestHelper.Request<Institution>(httpClient, Verb.Post, _institutionsUri, _configuration["CentOpsApiKey"], institutionPostBody).Result;

            // Create Dmr
            var dmrPostBody = JsonSerializer.Serialize(new Participant()
            {
                Name = "dmr1",
                InstitutionId = institution.Id,
                Host = "http://dmr/messages",
                Type = "Dmr",
                Status = "Active",
                ApiKey = "thisisareallylongkey"
            });
            var dmr = RequestHelper.Request<Participant>(httpClient, Verb.Post, _participantsUri, _configuration["CentOpsApiKey"], dmrPostBody).Result;

            // Create Bot1
            var bot1PostBody = JsonSerializer.Serialize(new Participant()
            {
                Name = "bot1",
                InstitutionId = institution.Id,
                Host = "http://bot1/dmr-api/messages",
                Type = "Chatbot",
                Status = "Active",
                ApiKey = "thisisareallylongkey"
            });
            var bot1 = RequestHelper.Request<Participant>(httpClient, Verb.Post, _participantsUri, _configuration["CentOpsApiKey"], bot1PostBody).Result;
        }

        public void Dispose()
        {
            // Do "global" teardown here; Only called once.

            // Setup
            using var httpClient = new HttpClient();

            // Get all participant for test id
            var participants = RequestHelper.Request<List<Participant>>(httpClient, Verb.Get, _participantsUri, _configuration["CentOpsApiKey"]).Result;

            // Delete each participant
            foreach (var participant in participants)
            {
                var deleteParticipantUri = new Uri($"{_participantsUri}/{participant.Id}");
                _ = RequestHelper.Request<List<Participant>>(httpClient, Verb.Delete, deleteParticipantUri, _configuration["CentOpsApiKey"]).Result;
            }

            // Delete institution
            var institutions = RequestHelper.Request<List<Institution>>(httpClient, Verb.Get, _institutionsUri, _configuration["CentOpsApiKey"]).Result;
            var testInstitution = institutions.FirstOrDefault(i => i.Name == $"TestInstitution{_testId}");
            var deleteInstitutionUri = new Uri($"{_institutionsUri}/{testInstitution.Id}");
            _ = RequestHelper.Request<List<Participant>>(httpClient, Verb.Delete, deleteInstitutionUri, _configuration["CentOpsApiKey"]).Result;
        }
    }
}

