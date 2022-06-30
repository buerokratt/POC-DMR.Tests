using System.Text.Json.Serialization;

namespace Tests.IntegrationTests.Models
{
    public class ChatMessage
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("sentBy")]
        public string SentBy { get; set; }

        [JsonPropertyName("sendTo")]
        public string SendTo { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("classification")]
        public string Classification { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("modelType")]
        public string ModelType { get; set; }
    }
}
