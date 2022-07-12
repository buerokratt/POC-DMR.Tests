using System.Text.Json.Serialization;

namespace Tests.IntegrationTests.Models
{
    public class Participant
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("institutionId")]
        public string InstitutionId { get; set; }

        [JsonPropertyName("host")]
        public string Host { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; }
    }
}

