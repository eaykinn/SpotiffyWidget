using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotiffyWidget.Models
{
    public class ProfileTrack
    {
        [JsonProperty("track")]
        public Track Track { get; set; }

        [JsonProperty("added_at")]
        public DateTime TimeAdded { get; set; }


    }
}
