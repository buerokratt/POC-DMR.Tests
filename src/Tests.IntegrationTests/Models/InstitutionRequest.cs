﻿using System.Text.Json.Serialization;

namespace Tests.IntegrationTests.Models
{
    public class InstitutionRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}

