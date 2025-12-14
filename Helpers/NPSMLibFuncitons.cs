using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using NPSMLib;

namespace SpotiffyWidget.Helpers
{
    public static class NPSMLibFunctions
    {
        private static NowPlayingSessionManager _player;
        private static NowPlayingSession _currentSession;
        private static MediaPlaybackDataSource _dataSource;

        public static string SongName { get; set; }
        public static string ArtistName { get; set; }
        public static string AlbumName { get; set; }
        public static BitmapFrame Image { get; set; }
        public static string MaxTime { get; set; }
        public static string CurrentTime { get; set; }
        public static int MaxSeconds { get; set; }
        public static int CurrentSecond { get; set; }
        public static bool IsPlaying { get; set; }

        private static double _lastPosition = -1;
        private static DateTime _lastUpdate = DateTime.Now;

        public static void InitializeSession()
        {
            _player = new NowPlayingSessionManager();

            var sessions = _player.GetSessions();
            var spotifySession = sessions.FirstOrDefault(x =>
                x.SourceAppId.Equals("Spotify.exe", StringComparison.OrdinalIgnoreCase)
                || x.SourceAppId.Contains("spotify", StringComparison.OrdinalIgnoreCase)
            );

            if (spotifySession == null)
            {
                SongName = "No Data";
                ArtistName = "No Data";
                AlbumName = "No Data";
                Image = null;
                return;
            }

            _player.SetCurrentSession(spotifySession.GetSessionInfo());
            _currentSession = _player.CurrentSession;
            _dataSource = _currentSession.ActivateMediaPlaybackDataSource();

            // 🎵 Şarkı bilgilerini ilk defa çek
            var info = _dataSource.GetMediaObjectInfo();
            SongName = info.Title ?? "No Data";
            ArtistName = info.Artist ?? "No Data";
            AlbumName = info.AlbumTitle ?? "No Data";

            // 🖼️ Albüm resmi
            using (var stream = _dataSource.GetThumbnailStream())
            {
                if (stream != null)
                {
                    Image = BitmapFrame.Create(
                        stream,
                        BitmapCreateOptions.None,
                        BitmapCacheOption.OnLoad
                    );
                }
            }
        }

        public static void UpdatePlayback()
        {
            if (_dataSource == null)
            {
                InitializeSession();
                return;
            }

            try
            {
                var mediaPBI = _dataSource.GetMediaPlaybackInfo();
                var mediaPBState = mediaPBI.PlaybackState;

                IsPlaying = mediaPBState == MediaPlaybackState.Playing;

                var timeline = _dataSource.GetMediaTimelineProperties();

                // Pozisyon kontrolü - threshold'u artırdık (0.5 saniye)
                double currentPos = timeline.Position.TotalSeconds;
                bool positionChanged = Math.Abs(currentPos - _lastPosition) > 0.5;

                if (positionChanged || _lastPosition < 0)
                {
                    // Yeni veri geldi → Spotify'dan güncelle
                    CurrentSecond = (int)currentPos;
                    MaxSeconds = (int)timeline.EndTime.TotalSeconds;
                    _lastPosition = currentPos;
                    _lastUpdate = DateTime.Now;

                    // Şarkı değişmiş olabilir - bilgileri güncelle
                    var info = _dataSource.GetMediaObjectInfo();
                    if (info.Title != SongName)
                    {
                        SongName = info.Title ?? "No Data";
                        ArtistName = info.Artist ?? "No Data";
                        AlbumName = info.AlbumTitle ?? "No Data";

                        // Albüm resmi güncelle
                        using (var stream = _dataSource.GetThumbnailStream())
                        {
                            if (stream != null)
                            {
                                Image = BitmapFrame.Create(
                                    stream,
                                    BitmapCreateOptions.None,
                                    BitmapCacheOption.OnLoad
                                );
                            }
                        }
                    }
                }
                else if (IsPlaying)
                {
                    // Yeni veri yok ama çalıyor → Manuel artır
                    double elapsedSeconds = (DateTime.Now - _lastUpdate).TotalSeconds;
                    if (elapsedSeconds >= 1.0)
                    {
                        // Her saniyede bir artır
                        CurrentSecond++;
                        
                        if (CurrentSecond > MaxSeconds)
                            CurrentSecond = MaxSeconds;

                        _lastUpdate = DateTime.Now;
                    }
                }

                // Süre metinlerini güncelle
                MaxTime = $"{timeline.EndTime.Minutes}:{timeline.EndTime.Seconds:D2}";
                CurrentTime = $"{(int)(CurrentSecond / 60)}:{CurrentSecond % 60:D2}";
            }
            catch
            {
                // Hata durumunda session'ı yeniden başlat
                _lastPosition = -1;
                InitializeSession();
            }
        }
    }
}
