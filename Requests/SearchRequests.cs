using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SpotiffyWidget.Helpers;
using SpotiffyWidget.Models;

namespace SpotiffyWidget.Requests
{
    public class SearchRequests
    {
        public static async Task<List<T>> Search<T>(
            string accessToken,
            string searchQuery,
            string type,
            CancellationToken cancellationToken
        )
        {
            string url =
                SpotifyEndPoints.Search.SearchEndpoint
                + $"query="
                + searchQuery
                + "&"
                + $"type="
                + type;

            var result = await SpotifyApiHelper.SendRequestAsync<SearchResponse>(
                url,
                accessToken,
                cancellationToken
            );

            if (result == null)
                return new List<T>();
            if (type == "track" && typeof(T) == typeof(Track))
                return result.Tracks.Items as List<T>;
            else if (type == "artist" && typeof(T) == typeof(Artist))
                return result.Artists.Items as List<T>;
            else if (type == "playlist" && typeof(T) == typeof(Playlist))
                return result.PlayLists.Items as List<T>;
            else
                return new List<T>();
        }
    }
}
