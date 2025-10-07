using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static SpotiffyWidget.Helpers.SpotifyAuth;

namespace SpotiffyWidget.Helpers
{
    public static class SpotifyAuth
    {
        public class SpTokens
        {
            public string access_token { get; set; }
            public string refresh_token { get; set; }
        }

        public static string GetAuthCode()
        {
            string redirectUri = Resources.CallBackUri.Uri;
            var listener = new HttpListener();
            listener.Prefixes.Add(redirectUri);
            listener.Start();

            /* HttpListenerContext context = null;*/
            Console.WriteLine("Waiting for redirect...");

            string scopes =
                "streaming user-read-playback-state user-modify-playback-state playlist-read-private user-top-read user-library-read";

            string authorizationUrl =
                SpotifyEndPoints.Auth.AuthUrl
                + $"?client_id="
                + Resources.ClientCredentials.ClientId
                + $"&response_type=code"
                + $"&redirect_uri={Uri.EscapeDataString(redirectUri)}"
                + $"&scope={Uri.EscapeDataString(scopes)}";

            Console.WriteLine("Go to this URL and authorize the application:");
            //MessageBox.Show(authorizationUrl);
            //Console.WriteLine(authorizationUrl);
            ProcessStartInfo linkx = new(authorizationUrl) { UseShellExecute = true };
            Process.Start(linkx);
            HttpListenerContext context;

            try
            {
                context = listener.GetContext();
            }
            catch (Exception)
            {
                throw;
            }

            string code = context.Request.QueryString["code"];

            if (code == null)
            {
                return "No auth";
            }
            var response = context.Response;

            string responseString = "Authorization successful. You can close this window.";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
            var z = context.Request.QueryString;
            listener.Stop();

            return code;
        }

        public static async Task<List<string>> GetAccessToken(string code)
        {
            List<string> tokens = new();
            string redirectUri = Resources.CallBackUri.Uri;

            using (var client = new HttpClient())
            {
                var authHeader = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        $"{Resources.ClientCredentials.ClientId}:{Resources.ClientCredentials.ClientSecret}"
                    )
                );
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

                var content = new FormUrlEncodedContent(
                    new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("code", code),
                        new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    }
                );

                var response = await client.PostAsync(SpotifyEndPoints.Auth.TokenUrl, content);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    SpTokens json = JsonSerializer.Deserialize<SpTokens>(responseContent);
                    tokens.Add(json.access_token);
                    tokens.Add(json.refresh_token);

                    Properties.Access.Default.AccessToken = tokens[0];
                    Properties.Access.Default.RefreshToken = tokens[1];
                    Properties.Access.Default.Save();
                    return tokens;
                }
                else
                {
                    Console.WriteLine("Error: " + responseContent);
                    return tokens;
                }
            }
        }

        public static async Task<string> RefreshAccessToken(string refreshToken)
        {
            using (var client = new HttpClient())
            {
                var authHeader = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        $"{Resources.ClientCredentials.ClientId}:{Resources.ClientCredentials.ClientSecret}"
                    )
                );
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);
                var content = new FormUrlEncodedContent(
                    new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "refresh_token"),
                        new KeyValuePair<string, string>("refresh_token", refreshToken),
                    }
                );
                var response = await client.PostAsync(SpotifyEndPoints.Auth.TokenUrl, content);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    SpTokens json = JsonSerializer.Deserialize<SpTokens>(responseContent);
                    Properties.Access.Default.AccessToken = json.access_token;
                    Properties.Access.Default.Save();
                    return json.access_token;
                }
                else
                {
                    Console.WriteLine("Error: " + responseContent);
                    return null;
                }
            }
        }

        public static async Task<bool> CheckToken(string accessToken)
        {
            accessToken = Properties.Access.Default.AccessToken;

            using (HttpClient clientS = new HttpClient())
            {
                clientS.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                HttpResponseMessage response = await clientS.GetAsync(SpotifyEndPoints.Profile.Me);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                    return false;
            }
        }
    }
}
