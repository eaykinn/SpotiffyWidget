using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NPSMLib;

namespace SpotiffyWidget.Helpers
{
    public static class NPSMLibFunctions
    {
        public static string SongName { get; set; } = null;
        public static string ArtistName { get; set; } = null;
        public static string AlbumName { get; set; } = null;
        public static BitmapFrame Image { get; set; } = null;
        public static string MaxTime { get; set; } = null;
        public static string CurrentTime { get; set; } = null;
        public static int MaxSeconds { get; set; } = 0;
        public static int CurrentSecond { get; set; } = 0;

        public static void GetNowPlayingInfo()
        {
            NowPlayingSessionManager player = new NowPlayingSessionManager();

            NowPlayingSession[] sessions = player.GetSessions();
            var sessionInfos = sessions
                .Where(x =>
                    x.SourceAppId == "Spotify.exe"
                    || x.SourceAppId.Contains("spotify")
                    || x.SourceAppId.Contains("Spotify")
                )
                .Select(x => x.GetSessionInfo())
                .ToList();
            if (sessionInfos.Count == 0)
            {
                SongName = "No Data";
                ArtistName = "No Data";
                AlbumName = "No Data";
                Image = null;
                return;
            }
            else
            {
                player.SetCurrentSession(sessionInfos[0]);
            }

            NowPlayingSession currentSession = player.CurrentSession;
            MediaPlaybackDataSource playnaclDataSource =
                currentSession.ActivateMediaPlaybackDataSource();

            using (Stream streamInfo = playnaclDataSource.GetThumbnailStream())
            {
                if (streamInfo != null)
                {
                    Image = BitmapFrame.Create(
                        streamInfo,
                        BitmapCreateOptions.None,
                        BitmapCacheOption.OnLoad
                    );
                }
            }

            MediaObjectInfo mediaInfo = playnaclDataSource.GetMediaObjectInfo();
            if (mediaInfo.Title != null)
            {
                SongName = mediaInfo.Title;
                ArtistName = mediaInfo.Artist;
                AlbumName = mediaInfo.AlbumTitle;
            }
            else
            {
                SongName = "No Data";
                ArtistName = "No Data";
                AlbumName = "No Data";
            }

            MediaTimelineProperties timeline = playnaclDataSource.GetMediaTimelineProperties();
            MaxTime =
                timeline.EndTime.Minutes.ToString() + ":" + timeline.EndTime.Seconds.ToString();
            CurrentTime =
                timeline.Position.Minutes.ToString() + ":" + timeline.Position.Seconds.ToString();

            CurrentSecond = (int)timeline.Position.TotalSeconds;

            MaxSeconds = (int)timeline.EndTime.TotalSeconds;

            return;
        }
    }
}
