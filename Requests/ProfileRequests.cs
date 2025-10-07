using Newtonsoft.Json;
using SpotiffyWidget.Helpers;
using SpotiffyWidget.Models;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using Track = SpotiffyWidget.Models.Track;

namespace SpotiffyWidget.Requests
{
    public static class ProfileRequests
    {   
        // user's top tracks  (time & limit parameters can be added)
        public static async Task<List<Track>> GetTopTracksAsync(string accessToken,int trackCount, CancellationToken cancellationToken)
        {
            string url = SpotifyEndPoints.Profile.TopTracks + "?" + $"limit=" + trackCount;
            var tracks = await SpotifyApiHelper.SendRequestAsync<Paging<Track>>(url, accessToken, cancellationToken);
            var trackItems = tracks.Items.ToList();
            return trackItems;
        }

        // user's tracks with added time (limit parameters can be added)
        public static async Task<List<ProfileTrack>> GetTracksAsync(string accessToken, int trackCount, CancellationToken cancellationToken)
        {
            string url = SpotifyEndPoints.Profile.Tracks +"?" + $"limit=" + trackCount; ;
            var tracks = await SpotifyApiHelper.SendRequestAsync<Paging<ProfileTrack>>(url, accessToken, cancellationToken);
            var trackItems = tracks.Items.ToList(); 
            return trackItems;
        }

        // user's top artists
        public static async Task<List<Artist>> GetTopArtistsAsync(string accessToken, CancellationToken cancellationToken)
        {
            string url = SpotifyEndPoints.Profile.TopArtists;
            var artists = await SpotifyApiHelper.SendRequestAsync<Paging<Artist>>(url, accessToken, cancellationToken);
            var artistsItems = artists.Items.ToList();
            return artistsItems;
        }

        // user's playlists
        public static async Task<List<Playlist>> GetUsersPlaylists(string accessToken, CancellationToken cancellationToken)
        {
            string url = SpotifyEndPoints.Profile.Playlists;
            var playlists = await SpotifyApiHelper.SendRequestAsync<Paging<Playlist>>(
                url,
                accessToken, cancellationToken
            );
            var playlistItems = playlists.Items.ToList();
            return playlistItems;
        }
    }
}
