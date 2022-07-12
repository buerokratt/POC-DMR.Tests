using System.Net;
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
        /// <typeparam name="T">Type that the result should be deserialised to and returned</typeparam>
        /// <param name="httpClient">A HttpClient to use/reuse</param>
        /// <param name="verb">Which Http verb to use</param>
        /// <param name="uri">The Uri to send the request to</param>
        /// <returns>Object representing deserialised result of type defined by T</returns>
        /// <exception cref="NotImplementedException">If verb is not in expected range.</exception>
        public static async Task<T> Request<T>(HttpClient httpClient, Verb verb, Uri uri, string body = "")
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            // Build content
            using var content = new StringContent(body, Encoding.UTF8, "application/json");

            // make request
            var response = verb switch
            {
                Verb.Post => await httpClient.PostAsync(uri, content).ConfigureAwait(false),
                Verb.Get => await httpClient.GetAsync(uri).ConfigureAwait(false),
                Verb.Delete => await httpClient.DeleteAsync(uri).ConfigureAwait(false),
                _ => throw new NotImplementedException(),
            };

            // Validate response
            _ = response.EnsureSuccessStatusCode();

            // Deserialise response
            T result = default;
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                var contentRaw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                result = JsonSerializer.Deserialize<T>(contentRaw);
            }

            return result;
        }
    }
}

