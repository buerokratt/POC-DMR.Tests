using System.Text;
using System.Text.Json;
using Tests.IntegrationTests.Models;

namespace Tests.IntegrationTests.Helpers
{
    public static class RequestHelper
    {
        /// <summary>
        /// Simple helper to handle http requests and deserialisation of result
        /// </summary>
        /// <typeparam name="T">Type that the result shoudl be deserialised to</typeparam>
        /// <param name="httpClient">A HttpClient to use/reuse</param>
        /// <param name="verb">Which Http verb to use</param>
        /// <param name="uri">The Uri to send the request to</param>
        /// <param name="uri">The Uri to send the request to</param>
        /// <param name="apiKey">The value to send as x-api-key header</param>
        /// <returns>Object representing deserialised result of type defined by T</returns>
        /// <exception cref="NotImplementedException">If verb is not in expected range.</exception>
        public static async Task<T> Request<T>(HttpClient httpClient, Verb verb, Uri uri, string apikey, string body = "")
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            // Add api key header unless it already exists
            if (!httpClient.DefaultRequestHeaders.Contains("x-api-key"))
            {
                httpClient.DefaultRequestHeaders.Add("x-api-key", apikey);
            }

            // Build content
            using var content = new StringContent(body, Encoding.UTF8, "application/json");

            // make request
            var response = verb switch
            {
                Verb.Post => await httpClient.PostAsync(uri, content).ConfigureAwait(false),
                Verb.Get => await httpClient.GetAsync(uri).ConfigureAwait(false),
                _ => throw new NotImplementedException(),
            };

            // Process response
            _ = response.EnsureSuccessStatusCode();
            var contentRaw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<T>(contentRaw);
            return result;
        }
    }
}

