using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests.IntegrationTests
{
    public static class Startup
    {
        public static void ConfigureHost(IHostBuilder hostBuilder)
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }

            var config = new ConfigurationBuilder()
                .AddJsonFile(@"appsettings.json", false, false)
                .AddEnvironmentVariables()
                .Build();

            _ = hostBuilder.ConfigureServices(services => services.AddSingleton<TestClients>());
            _ = hostBuilder.ConfigureHostConfiguration(builder => builder.AddConfiguration(config));
        }
    }
}