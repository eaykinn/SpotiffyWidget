using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HandyControl.Controls;
using Newtonsoft.Json;

namespace SpotiffyWidget.Helpers
{
    public static class SpotifyApiHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<T> SendRequestAsync<T>(
            string url,
            string accessToken,
            CancellationToken cancellationToken
        )
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            var result = JsonConvert.DeserializeObject<T>(json);
            return result;
        }

        public static async Task<HttpResponseMessage> PostAsync(
            string url,
            object body,
            string accessToken,
            CancellationToken cancellationToken
        )
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            if (body != null)
            {
                string json = JsonConvert.SerializeObject(body);

                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);

            return response;
        }

        public static async Task<HttpResponseMessage> PutAsync(
            string url,
            object body,
            string accessToken,
            CancellationToken cancellationToken
        )
        {
            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            if (body != null)
            {
                string json = JsonConvert.SerializeObject(body);

                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response;
        }

        public static async Task<HttpResponseMessage> DeleteAsync(
            string url,
            object body,
            string accessToken,
            CancellationToken cancellationToken
        )
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            if (body != null)
            {
                string json = JsonConvert.SerializeObject(body);

                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response;
        }
    }
}
