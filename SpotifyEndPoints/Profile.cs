using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotiffyWidget.SpotifyEndPoints
{
    public static class Profile
    {
        public static string Me { get; } = "https://api.spotify.com/v1/me";
        public static string Playlists { get; } = "https://api.spotify.com/v1/me/playlists";
        public static string TopTracks { get; } = "https://api.spotify.com/v1/me/top/tracks";
        public static string TopArtists { get; } = "https://api.spotify.com/v1/me/top/artists";
        public static string Tracks { get; } = "https://api.spotify.com/v1/me/tracks";
    }
}
