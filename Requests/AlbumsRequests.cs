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
    public static class AlbumsRequests
    {
        public static async Task<List<Track>> GetAlbumTracks(
            string accessToken,
            string id,
            CancellationToken cancellationToken
        )
        {
            string url = SpotifyEndPoints.AlbumEndpoints.AlbumTracks + "/" + id + "/tracks";
            var tracks = await SpotifyApiHelper.SendRequestAsync<Paging<Track>>(
                url,
                accessToken,
                cancellationToken
            );

            tracks ??= new Paging<Track>();

            return tracks.Items;
        }

        public static async Task<Album> GetAlbum(
            string accessToken,
            string id,
            CancellationToken cancellationToken
        )
        {
            string url = SpotifyEndPoints.AlbumEndpoints.AlbumTracks + "/" + id;
            var album = await SpotifyApiHelper.SendRequestAsync<Album>(
                url,
                accessToken,
                cancellationToken
            );

            album ??= new Album();

            return album;
        }
    }
}
