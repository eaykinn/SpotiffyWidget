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
                request.Content = new StringContent(
                    JsonConvert.SerializeObject(body),
                    Encoding.UTF8,
                    "application/json"
                );
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

        public static async Task<bool> GrantAccess()
        {
            if (Properties.Access.Default.AccessToken != "")
            {
                if (!await SpotifyAuth.CheckToken(Properties.Access.Default.AccessToken))
                {
                    string accessToken = await SpotifyAuth.RefreshAccessToken(
                        Properties.Access.Default.RefreshToken
                    );
                    if (accessToken == null)
                    {
                        Growl.Info("Could not refresh access token.");
                        return false;
                    }
                    return true;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                //token yok
                string authcode = SpotifyAuth.GetAuthCode();
                var accesstoken = await SpotifyAuth.GetAccessToken(authcode);
                if (accesstoken.Count == 0)
                {
                    Growl.Info("Could not get access token.");
                    return false;
                }
                return true;
            }
        }
    }
}
