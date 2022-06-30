using Microsoft.Extensions.Configuration;

namespace Tests.IntegrationTests
{
    public class ClassifyMessageTests
    {
        private readonly IConfiguration _configuration;

        public ClassifyMessageTests()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile(@"appsettings.json", false, false)
                .Build();
        }

        [Fact]
        public void GivenValidMessageReceivesValidResponse()
        {
            var result = _configuration["Bot1Url"];
            Assert.Equal("http://localhost:9012", result);
        }
    }
}