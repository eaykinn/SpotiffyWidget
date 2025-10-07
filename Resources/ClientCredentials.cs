using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotiffyWidget.Resources
{
    public static class ClientCredentials
    {
        public static readonly string ClientId =
            Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");

        public static readonly string ClientSecret =
            Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET");
    }
}
