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
    public static class ArtistsRequests
    {
        public static async Task<Artist> GetArtist(
            string accessToken,
            string id,
            CancellationToken cancellationToken
        )
        {
            string url = SpotifyEndPoints.Artist.Artists + "/" + id;
            var artist = await SpotifyApiHelper.SendRequestAsync<Artist>(
                url,
                accessToken,
                cancellationToken
            );

            artist ??= new Artist();
            return artist;
        }

        public static async Task<List<Album>> GetArtistAlbums(
            string accessToken,
            string id,
            CancellationToken cancellationToken
        )
        {
            string url = SpotifyEndPoints.Artist.Artists + "/" + id + "/albums";
            var albums = await SpotifyApiHelper.SendRequestAsync<Paging<Album>>(
                url,
                accessToken,
                cancellationToken
            );

            albums ??= new Paging<Album>();

            return albums.Items;
        }
    }
}
