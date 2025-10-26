using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpotiffyWidget.Models
{
    public class QueueResponse
    {
        [JsonProperty("currently_playing")]
        public Track CurrentlyPlaying { get; set; }

        [JsonProperty("queue")]
        public Track[] QueueTrack { get; set; }
    }
}
