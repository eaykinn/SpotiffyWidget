using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpotiffyWidget.Helpers
{
    public static class SpotifyApiHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<T> SendRequestAsync<T>(string url, string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<T>(json);
            return result;
        }

        public static async Task<HttpResponseMessage> PostAsync(
            string url,
            object body,
            string accessToken
        )
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(
                JsonConvert.SerializeObject(body),
                Encoding.UTF8,
                "application/json"
            );
            return await _httpClient.SendAsync(request);
        }
    }
}
