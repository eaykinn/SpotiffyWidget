using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotiffyWidget.SpotifyEndPoints
{
    public static class Tracks
    {
        public static string LikeDislike { get; set; } = "https://api.spotify.com/v1/me/tracks";
        public static string IsTracksSaved { get; set; } =
            "https://api.spotify.com/v1/me/tracks/contains";
    }
}
