using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SpotiffyWidget.Helpers;
using SpotiffyWidget.Models;

namespace SpotiffyWidget.Requests
{
    public static class PlayerRequests
    {
        // user's top tracks  (time & limit parameters can be added)
        public static async Task<Devices> GetDevices(
            string accessToken,
            CancellationToken cancellationToken
        )
        {
            string url = SpotifyEndPoints.Player.Devices;
            var devices = await SpotifyApiHelper.SendRequestAsync<Devices>(
                url,
                accessToken,
                cancellationToken
            );
            return devices;
        }

        public static async Task Play(
            string accessToken,
            object body,
            CancellationToken cancellationToken
        )
        {
            string url = SpotifyEndPoints.Player.Play;
            await SpotifyApiHelper.PutAsync(url, body, accessToken, cancellationToken);
        }

        public static async Task Pause(
            string accessToken,
            object body,
            CancellationToken cancellationToken
        )
        {
            string url = SpotifyEndPoints.Player.Pause;
            await SpotifyApiHelper.PutAsync(url, body, accessToken, cancellationToken);
        }

        public static async Task<PlayBackState> GetPlayBackState(
            string accessToken,
            CancellationToken cancellationToken
        )
        {
            string url = SpotifyEndPoints.Player.PlayBackState;
            var playBackState = await SpotifyApiHelper.SendRequestAsync<PlayBackState>(
                url,
                accessToken,
                cancellationToken
            );
            return playBackState;
        }
    }
}
