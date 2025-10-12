using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
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

            if (devices == null)
                return new Devices();
            return devices;
        }

        public static async Task<bool> Play(
            string accessToken,
            object body,
            CancellationToken cancellationToken
        )
        {
            string url = SpotifyEndPoints.Player.Play;
            var response = await SpotifyApiHelper.PutAsync(
                url,
                body,
                accessToken,
                cancellationToken
            );
            if (response == null)
                return false;
            return response.IsSuccessStatusCode ? true : false;
        }

        public static async Task<bool> TransferPlayBackState(
            string accessToken,
            object body,
            CancellationToken cancellationToken
        )
        {
            string url = SpotifyEndPoints.Player.PlayBackState;
            var response = await SpotifyApiHelper.PutAsync(
                url,
                body,
                accessToken,
                cancellationToken
            );
            if (response == null)
                return false;
            return response.IsSuccessStatusCode ? true : false;
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
            if (playBackState == null)
                return new PlayBackState();
            return playBackState;
        }

        public static async Task<bool> Next(string accessToken, CancellationToken cancellationToken)
        {
            object body = null;

            string url = SpotifyEndPoints.Player.Next;
            var response = await SpotifyApiHelper.PostAsync(
                url,
                body,
                accessToken,
                cancellationToken
            );
            if (response == null)
                return false;
            if (!response.IsSuccessStatusCode)
                return false;
            return true;
        }

        public static async Task<bool> Previous(
            string accessToken,
            CancellationToken cancellationToken
        )
        {
            string url = SpotifyEndPoints.Player.Previous;
            var response = await SpotifyApiHelper.PostAsync(
                url,
                null,
                accessToken,
                cancellationToken
            );
            if (response == null)
                return false;
            if (!response.IsSuccessStatusCode)
                return false;
            return true;
        }

        public static async Task<bool> AddQueue(
            string accessToken,
            string trackUri,
            CancellationToken cancellationToken
        )
        {
            string url = SpotifyEndPoints.Player.Queue + "?uri=" + trackUri;
            var response = await SpotifyApiHelper.PostAsync(
                url,
                null,
                accessToken,
                cancellationToken
            );
            if (response == null)
                return false;
            if (!response.IsSuccessStatusCode)
                return false;
            return true;
        }

        public static async Task<bool> SetVolume(
            string accessToken,
            int volume,
            CancellationToken cancellationToken
        )
        {
            string url = SpotifyEndPoints.Player.Volume + "?volume_percent=" + volume;
            var response = await SpotifyApiHelper.PutAsync(
                url,
                null,
                accessToken,
                cancellationToken
            );
            if (response == null)
                return false;
            return response.IsSuccessStatusCode ? true : false;
        }

        public static async Task<bool> ShufflePlayBack(
            string accessToken,
            bool isShuffle,
            CancellationToken cancellationToken
        )
        {
            string url = SpotifyEndPoints.Player.Shuffle + "?state=" + isShuffle;
            var response = await SpotifyApiHelper.PutAsync(
                url,
                null,
                accessToken,
                cancellationToken
            );
            if (response == null)
                return false;
            return response.IsSuccessStatusCode ? true : false;
        }
    }
}
