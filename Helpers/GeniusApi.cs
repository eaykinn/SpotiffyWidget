using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace SpotiffyWidget.Helpers
{
    public class GeniusApi
    {
        private readonly string clientAccessToken =
            "qDID9TaTLVHgjBfR0LyCu1YjZ6Xfv9TY4bgROy0rCcV_FQHd-juRd7SH3ut6L8UU";

        /// <summary>
        /// Şarkı adı ve sanatçıya göre Genius'tan şarkı sözü arar.
        /// </summary>
        /// <param name="songQuery">Şarkı adı</param>
        /// <param name="artistName">Sanatçı adı</param>
        /// <returns>Şarkı sözü veya hata mesajı</returns>
        public async Task<string> SearchSongAsync(string songQuery, string artistName)
        {
            // 1. Genius API ile arama yap
            string songUrl = await GetSongUrlAsync(songQuery, artistName);
            if (string.IsNullOrEmpty(songUrl))
                return "Lyrics not found.";

            // 2. Şarkı sözlerini Genius sayfasından çek
            string lyrics = await GetLyricsFromPageAsync(songUrl);
            return string.IsNullOrWhiteSpace(lyrics) ? "Şarkı sözü bulunamadı." : lyrics;
        }

        /// <summary>
        /// Genius API ile şarkı araması yapar ve en uygun şarkının URL'sini döner.
        /// </summary>
        private async Task<string> GetSongUrlAsync(string songQuery, string artistName)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                clientAccessToken
            );

            // Şarkı adı ve sanatçı adını birlikte ara
            string query = $"{songQuery} {artistName}";
            string url = $"https://api.genius.com/search?q={Uri.EscapeDataString(query)}";
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            string json = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);
            var hits = data["response"]?["hits"];
            if (hits == null)
                return null;

            int maxAnnCount = -1;
            string bestUrl = null;
            foreach (var item in hits)
            {
                var result = item["result"];
                if (result == null)
                    continue;

                string artistNames = result["artist_names"]?.ToString() ?? "";
                int annCount = result["annotation_count"]?.Value<int>() ?? 0;
                string urlCandidate = result["url"]?.ToString();

                if (
                    artistNames.Contains(artistName, StringComparison.OrdinalIgnoreCase)
                    && annCount > maxAnnCount
                )
                {
                    maxAnnCount = annCount;
                    bestUrl = urlCandidate;
                }
            }
            return bestUrl;
        }

        /// <summary>
        /// Genius şarkı sayfasından şarkı sözlerini çeker.
        /// </summary>
        private async Task<string> GetLyricsFromPageAsync(string songUrl)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(songUrl);

            // Farklı HTML yapıları için XPath'ler
            string[] xpaths = new[]
            {
                // En yaygın ve güncel Genius yapısı
                "//div[@data-lyrics-container='true']/p",
                "//div[@data-lyrics-container='true']",
                "//div[contains(@class,'Lyrics__Container')]",
                "//span[contains(@class,'ReferentFragment-desktop__Highlight')]",
                "//div[@class='lyrics']",
                "//p[ancestor::div[@data-lyrics-container='true']]",
                "//div[contains(@class,'lyrics-root')]//p",
            };

            foreach (var xpath in xpaths)
            {
                var nodes = doc.DocumentNode.SelectNodes(xpath);
                if (nodes != null && nodes.Count > 0)
                {
                    var sb = new StringBuilder();
                    string prevLine = null;
                    foreach (var node in nodes)
                    {
                        // Eğer node <p> ise, içeriğini işle
                        if (node.Name == "p")
                        {
                            string html = node
                                .InnerHtml.Replace("<br>", "\n")
                                .Replace("<br/>", "\n")
                                .Replace("<br />", "\n");
                            string[] lines = WebUtility.HtmlDecode(html).Split('\n');
                            foreach (var line in lines)
                            {
                                string trimmed = line.Trim();
                                if (
                                    string.IsNullOrEmpty(trimmed)
                                    || trimmed.StartsWith("[")
                                    || trimmed.Length < 2
                                        && !char.IsLetterOrDigit(trimmed.FirstOrDefault())
                                )
                                    continue;
                                if (prevLine == trimmed)
                                    continue;
                                sb.AppendLine(trimmed);
                                prevLine = trimmed;
                            }
                        }
                        else
                        {
                            // Diğer node tipleri için mevcut işleyişi koru
                            var sbNode = new StringBuilder();
                            foreach (var child in node.ChildNodes)
                            {
                                if (child.NodeType == HtmlNodeType.Text)
                                {
                                    string line = WebUtility.HtmlDecode(child.InnerText).Trim();
                                    if (!string.IsNullOrEmpty(line) && !line.StartsWith("["))
                                        sbNode.AppendLine(line);
                                }
                                else if (child.Name == "br")
                                {
                                    sbNode.AppendLine();
                                }
                            }
                            string[] lines = sbNode.ToString().Split('\n');
                            foreach (var line in lines)
                            {
                                string trimmed = line.Trim();
                                if (
                                    string.IsNullOrEmpty(trimmed)
                                    || trimmed.StartsWith("[")
                                    || trimmed.Length < 2
                                        && !char.IsLetterOrDigit(trimmed.FirstOrDefault())
                                )
                                    continue;
                                if (prevLine == trimmed)
                                    continue;
                                sb.AppendLine(trimmed);
                                prevLine = trimmed;
                            }
                        }
                    }
                    if (sb.Length > 0)
                        return sb.ToString();
                }
            }
            return null;
        }
    }
}
