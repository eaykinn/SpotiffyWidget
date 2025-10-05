using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotiffyWidget.SpotifyEndPoints
{
    public static class Search
    {
        public static string SearchEndpoint { get; } =
            "https://api.spotify.com/v1/search?offset=0&limit=20&";
    }
}
