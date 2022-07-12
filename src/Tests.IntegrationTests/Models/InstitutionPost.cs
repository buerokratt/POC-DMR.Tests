using System.Text.Json.Serialization;

namespace Tests.IntegrationTests.Models
{
    public class InstitutionPost
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}

