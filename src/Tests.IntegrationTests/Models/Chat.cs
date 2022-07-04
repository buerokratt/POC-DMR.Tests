using System.Text.Json.Serialization;

namespace Tests.IntegrationTests.Models
{
    public class Chat
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

}
