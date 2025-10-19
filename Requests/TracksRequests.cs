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
    public class TracksRequests
    {
        public static async Task<bool> LikeSong(
            string accessToken,
            object body,
            CancellationToken cancellationToken
        )
        {
            string url = SpotifyEndPoints.Tracks.LikeDislike;

            var response = await SpotifyApiHelper.PutAsync(
                url,
                body,
                accessToken,
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
                return false;
            return true;
        }

        public static async Task<bool> RemoveSong(
            string accessToken,
            object body,
            CancellationToken cancellationToken
        )
        {
            string url = SpotifyEndPoints.Tracks.LikeDislike;

            var response = await SpotifyApiHelper.DeleteAsync(
                url,
                body,
                accessToken,
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
                return false;
            return true;
        }

        public static async Task<List<bool>> CheckTracksIsSaved(
            string accessToken,
            string body,
            CancellationToken cancellationToken
        )
        {
            string url = SpotifyEndPoints.Tracks.IsTracksSaved + "?ids=" + body;

            var response = await SpotifyApiHelper.SendRequestAsync<List<bool>>(
                url,
                accessToken,
                cancellationToken
            );

            return response;
        }
    }
}
