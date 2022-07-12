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
            var institutionsUri = new Uri($"{_configuration["CentOpsUrl"]}/admin/institutions");

            // Create Institution
            using var httpClient = new HttpClient();
            var postBody = new InstitutionPost()
            {
                Name = "TestInstitution",
            };
            var postBodyString = JsonSerializer.Serialize(postBody);
            var institution = RequestHelper.Request<Institution>(
                httpClient,
                Verb.Post,
                institutionsUri,
                _configuration["CentOpsApiKey"],
                postBodyString)
                .Result;
        }

        public void Dispose()
        {
            // Do "global" teardown here; Only called once.
        }
    }
}