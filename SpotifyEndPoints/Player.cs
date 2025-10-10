using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace SpotiffyWidget.SpotifyEndPoints
{
    public static class Player
    {
        public static string Play { get; } = "https://api.spotify.com/v1/me/player/play";
        public static string PlayBackState { get; } = "https://api.spotify.com/v1/me/player";

        public static string Pause { get; } = "https://api.spotify.com/v1/me/player/pause";

        public static string Repeat { get; } = "https://api.spotify.com/v1/me/player/repeat";

        public static string Volume { get; } = "https://api.spotify.com/v1/me/player/volume";

        public static string Shuffle { get; } = "https://api.spotify.com/v1/me/player/shuffle";

        public static string Queue { get; } = "https://api.spotify.com/v1/me/player/queue";

        public static string Devices { get; } = "https://api.spotify.com/v1/me/player/devices";
        public static string Next { get; } = "https://api.spotify.com/v1/me/player/next";
        public static string Previous { get; } = "https://api.spotify.com/v1/me/player/previous";
    }
}
