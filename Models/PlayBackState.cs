using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpotiffyWidget.Models
{
    public class PlayBackState
    {
        [JsonProperty("is_playing")]
        public bool IsPlaying { get; set; }
    }
}
