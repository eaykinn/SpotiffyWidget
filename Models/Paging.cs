using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpotiffyWidget.Models
{
    public class Paging<T>
    {
        [JsonProperty("items")]
        public List<T> Items { get; set; }
    }

    public class SearchResponse
    {
        [JsonProperty("tracks")]
        public Paging<Track> Tracks { get; set; }

        [JsonProperty("artists")]
        public Paging<Artist> Artists { get; set; }

        [JsonProperty("playlists")]
        public Paging<Playlist> PlayLists { get; set; }
    }

    public class Devices
    {
        [JsonProperty("devices")]
        public List<Device> DeviceList { get; set; }
    }
}
