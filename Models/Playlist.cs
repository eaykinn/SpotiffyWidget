using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpotiffyWidget.Models
{
    public class Playlist
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("images")]
        public List<Image> Images { get; set; }

        [JsonProperty("owner")]
        public UserProfile Owner { get; set; }

        [JsonProperty("tracks")]
        public TrackInfo TrackInfo { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
