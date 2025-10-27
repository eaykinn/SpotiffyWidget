using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography; // PKCE için eklendi
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using HandyControl.Controls;
using Newtonsoft.Json.Linq;
using SpotiffyWidget.Models;
using SpotiffyWidget.Requests;
using static SpotiffyWidget.Helpers.SpotifyAuth;

namespace SpotiffyWidget.Helpers
{
    public static class SpotifyAuth
    {
        // GetAuthCode'da oluşturulan verifier'ı GetAccessToken'da kullanmak için geçici olarak saklar
        private static string _pkceCodeVerifier;

        public class SpTokens
        {
            public string access_token { get; set; }
            public string refresh_token { get; set; }
        }

        public static async Task<string> GetAuthCode()
        {
            string redirectUri = Resources.CallBackUri.Uri;
            var listener = new HttpListener();
            listener.Prefixes.Add(redirectUri);
            listener.Start();

            Console.WriteLine("Waiting for redirect...");

            // --- PKCE için Eklendi ---
            _pkceCodeVerifier = GenerateCodeVerifier();
            string codeChallenge = GenerateCodeChallenge(_pkceCodeVerifier);
            string codeChallengeMethod = "S256";
            // -------------------------

            string scopes =
                "streaming user-read-playback-state user-modify-playback-state playlist-read-private user-top-read user-library-read user-library-modify";

            // --- URL, PKCE parametreleri ile güncellendi ---
            string authorizationUrl =
                SpotifyEndPoints.Auth.AuthUrl
                + $"?client_id="
                + Resources.ClientCredentials.ClientId
                + $"&response_type=code"
                + $"&redirect_uri={Uri.EscapeDataString(redirectUri)}"
                + $"&scope={Uri.EscapeDataString(scopes)}"
                + $"&code_challenge={codeChallenge}"                 // Eklendi
                + $"&code_challenge_method={codeChallengeMethod}";   // Eklendi

            Console.WriteLine("Go to this URL and authorize the application:");
            ProcessStartInfo linkx = new(authorizationUrl) { UseShellExecute = true };

            await Task.Run(() => Process.Start(linkx));

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
                // --- PKCE Değişikliği: Basic Auth (ClientSecret) kaldırıldı ---
                // var authHeader = Convert.ToBase64String(
                //     Encoding.UTF8.GetBytes(
                //         $"{Resources.ClientCredentials.ClientId}:{Resources.ClientCredentials.ClientSecret}"
                //     )
                // );
                // client.DefaultRequestHeaders.Authorization =
                //     new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);
                // -----------------------------------------------------------

                // --- PKCE Değişikliği: İstek gövdesi güncellendi ---
                var content = new FormUrlEncodedContent(
                    new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("code", code),
                        new KeyValuePair<string, string>("redirect_uri", redirectUri),
                        new KeyValuePair<string, string>("client_id", Resources.ClientCredentials.ClientId), // Eklendi
                        new KeyValuePair<string, string>("code_verifier", _pkceCodeVerifier) // Eklendi
                    }
                );
                
                // Kullanıldıktan sonra verifier'ı temizle
                _pkceCodeVerifier = null; 
                // -------------------------------------------------

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
                // --- PKCE Değişikliği: Basic Auth (ClientSecret) kaldırıldı ---
                // var authHeader = Convert.ToBase64String(
                //     Encoding.UTF8.GetBytes(
                //         $"{Resources.ClientCredentials.ClientId}:{Resources.ClientCredentials.ClientSecret}"
                //     )
                // );
                // client.DefaultRequestHeaders.Authorization =
                //     new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);
                // -----------------------------------------------------------

                // --- PKCE Değişikliği: İstek gövdesi güncellendi ---
                var content = new FormUrlEncodedContent(
                    new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "refresh_token"),
                        new KeyValuePair<string, string>("refresh_token", refreshToken),
                        new KeyValuePair<string, string>("client_id", Resources.ClientCredentials.ClientId) // Eklendi
                    }
                );
                // -------------------------------------------------

                var response = await client.PostAsync(SpotifyEndPoints.Auth.TokenUrl, content);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    SpTokens json = JsonSerializer.Deserialize<SpTokens>(responseContent);
                    Properties.Access.Default.AccessToken = json.access_token;
                    // Not: Refresh token isteği bazen yeni bir refresh_token döndürebilir,
                    // onu da saklamak iyi bir pratik olabilir, ancak Spotify'da bu genellikle aynı kalır.
                    // if (json.refresh_token != null) { Properties.Access.Default.RefreshToken = json.refresh_token; }
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
            // Bu metodun değişmesine gerek yok.
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

        public static async Task<bool> GrantAccess()
        {
            // Bu metodun değişmesine gerek yok.
            // Zaten diğer metodları çağırdığı ve imzaları değişmediği için
            // otomatik olarak PKCE akışını kullanacaktır.
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
                        string authcode = await SpotifyAuth.GetAuthCode();
                        var accesstoken = await SpotifyAuth.GetAccessToken(authcode);

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
                string authcode = await SpotifyAuth.GetAuthCode();
                var accesstoken = await SpotifyAuth.GetAccessToken(authcode);
                if (accesstoken.Count == 0)
                {
                    Growl.Info("Could not get access token.");
                    return false;
                }
                return true;
            }
        }

        public static async Task<bool> CheckDevice()
        {
            // Bu metodun değişmesine gerek yok.
            CancellationService.Reset();
            var cancellationToken = CancellationService.Token;

            var devices = await PlayerRequests.GetDevices(
                Properties.Access.Default.AccessToken,
                cancellationToken
            );

            if (devices == null || devices.DeviceList == null)
                return false;

            if (devices.DeviceList.Count == 0)
            {
                HandyControl.Controls.MessageBox.Show(
                    "No active device found. Please open Spotify on one of your devices.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return false;
            }
            else
            {
                if (!devices.DeviceList.Any(x => x.IsActive))
                {
                    bool success = await PlayerRequests.TransferPlayBackState(
                        Properties.Access.Default.AccessToken,
                        new { device_ids = new string[] { devices.DeviceList[0].Id }, play = true },
                        cancellationToken
                    );

                    if (!success)
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        // --- PKCE için Gerekli Yardımcı Metodlar ---
        // Bu 3 metodu sınıfınızın içine (en alta) ekleyin.

        /// <summary>
        /// Rastgele bir "code verifier" oluşturur.
        /// </summary>
        private static string GenerateCodeVerifier()
        {
            var bytes = new byte[64]; // 64 byte (43-128 karakter arası olmalı)
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Base64UrlEncode(bytes);
        }

        /// <summary>
        /// Verilen "code verifier"ı kullanarak bir "code challenge" (SHA256) oluşturur.
        /// </summary>
        private static string GenerateCodeChallenge(string codeVerifier)
        {
            using (var sha256 = SHA256.Create())
            {
                var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                return Base64UrlEncode(challengeBytes);
            }
        }

        /// <summary>
        /// Standart Base64 string'i Base64-URL-safe formata dönüştürür.
        /// ( +, / karakterlerini değiştirir ve = padding'ini kaldırır)
        /// </summary>
        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
    }
}