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
    public static class PlayListsRequests
    {
        public static async Task<List<ProfileTrack>> GetPlayListTracks(
            string accessToken,
            string id,
            CancellationToken cancellationToken
        )
        {
            string url = SpotifyEndPoints.PlayListEndPoints.PlayListTracks + "/" + id + "/tracks";
            var tracks = await SpotifyApiHelper.SendRequestAsync<Paging<ProfileTrack>>(
                url,
                accessToken,
                cancellationToken
            );

            tracks ??= new Paging<ProfileTrack>();

            return tracks.Items;
        }

        public static async Task<Playlist> GetPlayList(
            string accessToken,
            string id,
            CancellationToken cancellationToken
        )
        {
            string url = SpotifyEndPoints.PlayListEndPoints.PlayListTracks + "/" + id;
            var playlist = await SpotifyApiHelper.SendRequestAsync<Playlist>(
                url,
                accessToken,
                cancellationToken
            );

            playlist ??= new Playlist();

            return playlist;
        }
    }
}
