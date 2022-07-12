﻿using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Tests.IntegrationTests.Models;
using Tests.IntegrationTests.Extensions;

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
            var institution = _client.Request<Institution>(Verb.Post, _institutionsUri, institutionPostBody).Result;

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
            _ = _client.Request<Participant>(Verb.Post, _participantsUri, dmrPostBody).Result;

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
            _ = _client.Request<Participant>(Verb.Post, _participantsUri, bot1PostBody).Result;
        }

        public void Dispose()
        {
            // Do "global" teardown here; Only called once.

            // Get all participant for test id
            var participants = _client.Request<List<Participant>>(Verb.Get, _participantsUri).Result;

            // Delete each participant
            foreach (var participant in participants)
            {
                var deleteParticipantUri = new Uri($"{_participantsUri}/{participant.Id}");
                _ = _client.Request<List<Participant>>(Verb.Delete, deleteParticipantUri).Result;
            }

            // Delete institution
            var institutions = _client.Request<List<Institution>>(Verb.Get, _institutionsUri).Result;
            var testInstitution = institutions.FirstOrDefault(i => i.Name == $"TestInstitution{_testId}");
            var deleteInstitutionUri = new Uri($"{_institutionsUri}/{testInstitution.Id}");
            _ = _client.Request<List<Participant>>(Verb.Delete, deleteInstitutionUri).Result;

            // Dispose
            _client.Dispose();
        }
    }
}
