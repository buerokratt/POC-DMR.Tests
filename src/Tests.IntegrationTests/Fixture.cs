using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Tests.IntegrationTests.Helpers;
using Tests.IntegrationTests.Models;

namespace Tests.IntegrationTests
{
    public sealed class Fixture : IDisposable
    {
        private readonly IConfiguration _configuration;

        public Fixture(IConfiguration configuration)
        {
            // Do "global" initialization here; Only called once.
            _configuration = configuration;
            using var httpClient = new HttpClient();
            var institutionsUri = new Uri($"{_configuration["CentOpsUrl"]}/admin/institutions");
            var participantsUri = new Uri($"{_configuration["CentOpsUrl"]}/admin/participants");

            // Create Institution
            var institutionPostBody = new InstitutionPost()
            {
                Name = "TestInstitution",
            };
            var institutionPostBodyString = JsonSerializer.Serialize(institutionPostBody);
            var institution = RequestHelper.Request<Institution>(httpClient, Verb.Post, institutionsUri, _configuration["CentOpsApiKey"], institutionPostBodyString).Result;

            // Create Dmr
        }

        public void Dispose()
        {
            // Do "global" teardown here; Only called once.
        }
    }
}