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
        private readonly HttpClient _client;

        public CentOpsFixture(IConfiguration configuration)
        {
            // Do "global" initialization here; Only called once.

            // Setup
            _configuration = configuration;
            _testId = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.CurrentCulture);
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("x-api-key", _configuration["CentOpsApiKey"]);
            _institutionsUri = new Uri($"{_configuration["CentOpsUrl"]}/admin/institutions");
            _participantsUri = new Uri($"{_configuration["CentOpsUrl"]}/admin/participants");

            // Create Institution
            var institutionPostBody = JsonSerializer.Serialize(new InstitutionRequest()
            {
                Name = $"TestInstitution{_testId}",
            });
            var institution = RequestHelper.Request<Institution>(_client, Verb.Post, _institutionsUri, institutionPostBody).Result;

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
            _ = RequestHelper.Request<Participant>(_client, Verb.Post, _participantsUri, dmrPostBody).Result;

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
            _ = RequestHelper.Request<Participant>(_client, Verb.Post, _participantsUri, bot1PostBody).Result;
        }

        public void Dispose()
        {
            // Do "global" teardown here; Only called once.

            // Get all participant for test id
            var participants = RequestHelper.Request<List<Participant>>(_client, Verb.Get, _participantsUri).Result;

            // Delete each participant
            foreach (var participant in participants)
            {
                var deleteParticipantUri = new Uri($"{_participantsUri}/{participant.Id}");
                _ = RequestHelper.Request<List<Participant>>(_client, Verb.Delete, deleteParticipantUri).Result;
            }

            // Delete institution
            var institutions = RequestHelper.Request<List<Institution>>(_client, Verb.Get, _institutionsUri).Result;
            var testInstitution = institutions.FirstOrDefault(i => i.Name == $"TestInstitution{_testId}");
            var deleteInstitutionUri = new Uri($"{_institutionsUri}/{testInstitution.Id}");
            _ = RequestHelper.Request<List<Participant>>(_client, Verb.Delete, deleteInstitutionUri).Result;

            // Dispose
            _client.Dispose();
        }
    }
}

