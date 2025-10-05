using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotiffyWidget.SpotifyEndPoints
{
    public static class Auth
    {
        public static string AuthUrl { get; set; } = "https://accounts.spotify.com/authorize";

        public static string TokenUrl { get; set; } = "https://accounts.spotify.com/api/token";
    }
}
