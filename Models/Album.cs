using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpotiffyWidget.Models
{
    public class Album
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("images")]
        public List<Image> Image { get; set; }

        [JsonProperty("total_tracks")]
        public int TotalTracks { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }
    }
}
