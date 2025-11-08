using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpotiffyWidget.Models
{
    public class ArtistTopTracks
    {
        [JsonProperty("tracks")]
        public List<Track> Tracks { get; set; }
    }
}
