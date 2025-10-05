using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using Newtonsoft.Json;
using SpotiffyWidget.Helpers;
using SpotiffyWidget.Models;
using Track = SpotiffyWidget.Models.Track;

namespace SpotiffyWidget.Requests
{
    public static class ProfileRequests
    {
        public static async Task<List<Track>> GetTopTracksAsync(string accessToken)
        {
            string url = SpotifyEndPoints.Profile.TopTracks;
            var tracks = await SpotifyApiHelper.SendRequestAsync<Paging<Track>>(url, accessToken);
            var trackItems = tracks.Items.ToList();
            return trackItems;
        }

        public static async Task<List<Artist>> GetTopArtistsAsync(string accessToken)
        {
            string url = SpotifyEndPoints.Profile.TopArtists;
            var artists = await SpotifyApiHelper.SendRequestAsync<Paging<Artist>>(url, accessToken);
            var artistsItems = artists.Items.ToList();
            return artistsItems;
        }

        public static async Task<List<Playlist>> GetUsersPlaylists(string accessToken)
        {
            string url = SpotifyEndPoints.Profile.Playlists;
            var playlists = await SpotifyApiHelper.SendRequestAsync<Paging<Playlist>>(
                url,
                accessToken
            );
            var playlistItems = playlists.Items.ToList();
            return playlistItems;
        }
    }
}
