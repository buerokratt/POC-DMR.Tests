using Microsoft.Extensions.Configuration;

namespace Tests.IntegrationTests
{
    public class TestClients : IDisposable
    {
        public HttpClient CentOpsAdminClient { get; }

        public HttpClient MockBotChatClient { get; }

        private bool _disposed;

        public TestClients(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            CentOpsAdminClient = new HttpClient();
            CentOpsAdminClient.DefaultRequestHeaders.Add("x-api-key", configuration["CentOpsApiKey"]);

            MockBotChatClient = new HttpClient();
            MockBotChatClient.DefaultRequestHeaders.Add("x-api-key", configuration["Bot1ApiKey"]);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                CentOpsAdminClient.Dispose();
                MockBotChatClient.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
