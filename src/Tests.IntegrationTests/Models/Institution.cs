using System.Text.Json.Serialization;

namespace Tests.IntegrationTests.Models
{
    public class Institution
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}

