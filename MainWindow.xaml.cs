using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HandyControl.Controls;
using HandyControl.Themes;
using HandyControl.Tools;
using SpotiffyWidget.Cards;
using SpotiffyWidget.Helpers;
using SpotiffyWidget.Models;
using TabControl = HandyControl.Controls.TabControl;
using TabItem = HandyControl.Controls.TabItem;

namespace SpotiffyWidget
{
    public partial class MainWindow
    {
        private bool _tracksOnLoad;
        private bool _artistsOnLoad;
        private bool _playlistsOnLoad;

        public MainWindow()
        {
            InitializeComponent();
            _tracksOnLoad = true;
            _artistsOnLoad = true;
            _playlistsOnLoad = true;
            Loaded += (s, e) => BlurHelper.EnableBlur(this);
        }

        #region Change Theme
        private void ButtonClick(object sender, RoutedEventArgs e) => CheckAccess();

        private void ButtonSkins_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button)
            {
                PopupConfig.IsOpen = false;
                if (button.Tag is ApplicationTheme tag)
                {
                    ((App)Application.Current).UpdateTheme(tag);
                }
                else if (button.Tag is Brush accentTag)
                {
                    ((App)Application.Current).UpdateAccent(accentTag);
                }
                else if (button.Tag is "Picker")
                {
                    var picker = SingleOpenHelper.CreateControl<ColorPicker>();
                    var window = new PopupWindow
                    {
                        PopupElement = picker,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        AllowsTransparency = true,
                        WindowStyle = WindowStyle.None,
                        MinWidth = 0,
                        MinHeight = 0,
                        Title = "Select Accent Color",
                    };

                    picker.SelectedColorChanged += delegate
                    {
                        ((App)Application.Current).UpdateAccent(picker.SelectedBrush);
                        window.Close();
                    };
                    picker.Canceled += delegate
                    {
                        window.Close();
                    };
                    window.Show();
                }
            }
        }
        #endregion

        private async void LoadTopArtists()
        {
            if (!await SpotifyAuth.GrantAccess())
                return;

            CancellationService.Reset();
            var cancellationToken = CancellationService.Token;
            ArtistListBox.Items.Clear();
            LoadingPanel.Visibility = Visibility.Visible;

            try
            {
                var artists = await Requests.ProfileRequests.GetTopArtistsAsync(
                    Properties.Access.Default.AccessToken,
                    cancellationToken
                );
                cancellationToken.ThrowIfCancellationRequested();
                var artistNames = artists.Select(a => a.Name).ToList();

                Dispatcher.Invoke(() =>
                {
                    foreach (var s in artists)
                    {
                        ArtistCard card = new ArtistCard();
                        card.Name.Text = s.Name;

                        card.Cover.Source = new System.Windows.Media.Imaging.BitmapImage(
                            new Uri(s.Images.FirstOrDefault().Url)
                        );

                        ArtistListBox.Items.Add(card);
                    }
                });
                // 3) Search
                /*var searchArtists = await Requests.SearchRequests.Search<Artist>(
                    Properties.Access.Default.AccessToken,
                    "Imagine Dragons",
                    "artist",
                    cancellationToken
                );
                cancellationToken.ThrowIfCancellationRequested();
                var searchArtistNames = searchArtists.Select(a => a.Name).ToList();


             

                // Tek seferde UI güncellemesi
                Dispatcher.Invoke(() =>
                {
                    TracksListBox.Items.Clear();
                    foreach (var s in allTracks)
                    {
                        TrackCard card = new TrackCard();
                        card.Name.Text = s.Track.Name;
                        card.Artist.Text = s.Track.Artists.FirstOrDefault().Name;
                        card.Album.Text = s.Track.Album.Name;
                        card.Cover.Source = new System.Windows.Media.Imaging.BitmapImage(
                            new Uri(s.Track.Album.Images.FirstOrDefault().Url)
                        );

                        TracksListBox.Items.Add(card);
                    }
                });*/
            }
            catch (OperationCanceledException oce)
            {
                // Gerçekten bizim token'ımız tarafından iptal mi yoksa başka bir sebepten mi?
                if (cancellationToken.IsCancellationRequested)
                    Growl.Info("İşlem kullanıcı tarafından iptal edildi.");
                else
                    Growl.Warning(
                        "İstek zaman aşımına uğradı veya dışarıdan bir iptal oldu: " + oce.Message
                    );
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message);
            }
            finally
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
            }
        }

        private async void LoadAllTracks()
        {
            if (!await SpotifyAuth.GrantAccess())
                return;

            CancellationService.Reset();
            var cancellationToken = CancellationService.Token;
            TracksListBox.Items.Clear();
            LoadingPanel.Visibility = Visibility.Visible;

            try
            {
                // 5) All tracks
                var allTracks = await Requests.ProfileRequests.GetTracksAsync(
                    Properties.Access.Default.AccessToken,
                    50,
                    cancellationToken
                );
                cancellationToken.ThrowIfCancellationRequested();

                // Tek seferde UI güncellemesi
                Dispatcher.Invoke(() =>
                {
                    foreach (var s in allTracks)
                    {
                        TrackCard card = new TrackCard();
                        card.Name.Text = s.Track.Name;
                        card.Artist.Text = s.Track.Artists.FirstOrDefault().Name;
                        card.Album.Text = s.Track.Album.Name;
                        card.TrackUri = s.Track.Uri;
                        card.TrackId = s.Track.Id;
                        card.Cover.Source = new System.Windows.Media.Imaging.BitmapImage(
                            new Uri(s.Track.Album.Images.FirstOrDefault().Url)
                        );

                        TracksListBox.Items.Add(card);
                    }
                });
            }
            catch (OperationCanceledException oce)
            {
                // Gerçekten bizim token'ımız tarafından iptal mi yoksa başka bir sebepten mi?
                if (cancellationToken.IsCancellationRequested)
                    Growl.Info("İşlem kullanıcı tarafından iptal edildi.");
                else
                    Growl.Warning(
                        "İstek zaman aşımına uğradı veya dışarıdan bir iptal oldu: " + oce.Message
                    );
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message);
            }
            finally
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
            }
        }

        private async void LoadMyPlayLists()
        {
            if (!await SpotifyAuth.GrantAccess())
                return;

            CancellationService.Reset();
            var cancellationToken = CancellationService.Token;
            MyPlayLists.Items.Clear();
            LoadingPanel.Visibility = Visibility.Visible;

            try
            {
                // 4) Playlists
                var playlists = await Requests.ProfileRequests.GetUsersPlaylists(
                    Properties.Access.Default.AccessToken,
                    cancellationToken
                );
                cancellationToken.ThrowIfCancellationRequested();

                // Tek seferde UI güncellemesi
                Dispatcher.Invoke(() =>
                {
                    foreach (var s in playlists)
                    {
                        MyPlayListsCard card = new MyPlayListsCard();
                        card.PlayListName.Text = s.Name;
                        card.Owner.Text = s.Owner.DisplayName;
                        card.Cover.Source = new System.Windows.Media.Imaging.BitmapImage(
                            new Uri(s.Images.FirstOrDefault().Url)
                        );
                        card.NumberOfTracks.Text = s.TrackInfo.Total.ToString() + " Songs";
                        MyPlayLists.Items.Add(card);
                    }
                });
            }
            catch (OperationCanceledException oce)
            {
                // Gerçekten bizim token'ımız tarafından iptal mi yoksa başka bir sebepten mi?
                if (cancellationToken.IsCancellationRequested)
                    Growl.Info("İşlem kullanıcı tarafından iptal edildi.");
                else
                    Growl.Warning(
                        "İstek zaman aşımına uğradı veya dışarıdan bir iptal oldu: " + oce.Message
                    );
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message);
            }
            finally
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
            }
        }

        private async void LoadTopAllTracks()
        {
            if (!await SpotifyAuth.GrantAccess())
                return;

            CancellationService.Reset();
            var cancellationToken = CancellationService.Token;
            TracksListBox.Items.Clear();
            LoadingPanel.Visibility = Visibility.Visible;

            try
            {
                // 5) All tracks
                var allTracks = await Requests.ProfileRequests.GetTopTracksAsync(
                    Properties.Access.Default.AccessToken,
                    10,
                    cancellationToken
                );
                cancellationToken.ThrowIfCancellationRequested();

                // Tek seferde UI güncellemesi
                Dispatcher.Invoke(() =>
                {
                    foreach (var s in allTracks)
                    {
                        TrackCard card = new TrackCard();
                        card.Name.Text = s.Name;
                        card.Artist.Text = s.Artists.FirstOrDefault().Name;
                        card.Album.Text = s.Album.Name;
                        card.TrackUri = s.Uri;
                        card.TrackId = s.Id;
                        card.Cover.Source = new System.Windows.Media.Imaging.BitmapImage(
                            new Uri(s.Album.Images.FirstOrDefault().Url)
                        );

                        TracksListBox.Items.Add(card);
                    }
                });
            }
            catch (OperationCanceledException oce)
            {
                // Gerçekten bizim token'ımız tarafından iptal mi yoksa başka bir sebepten mi?
                if (cancellationToken.IsCancellationRequested)
                    Growl.Info("İşlem kullanıcı tarafından iptal edildi.");
                else
                    Growl.Warning(
                        "İstek zaman aşımına uğradı veya dışarıdan bir iptal oldu: " + oce.Message
                    );
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message);
            }
            finally
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            CancellationService.Cancel();
            Growl.Info("İşlem iptal edildi.");
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                var tab = MainTabs.SelectedItem as TabItem;
                if (tab != null)
                {
                    switch (tab.Header.ToString())
                    {
                        case "Tracks":
                            if (_tracksOnLoad)
                            {
                                _tracksOnLoad = false;
                                TrackSearchBar.Text = "";
                                MyTracks.IsChecked = true;
                                LoadAllTracks();
                            }

                            break;
                        case "Artists":
                            if (_artistsOnLoad)
                            {
                                _artistsOnLoad = false;
                                ArtistSearchBar.Text = "";
                                MyTopArtists.IsChecked = true;
                                LoadTopArtists();
                            }
                            break;
                        case "PlayLists":
                            if (_playlistsOnLoad)
                            {
                                _playlistsOnLoad = false;
                                PlayListSearchBar.Text = "";
                                MyPlayListsRadioButton.IsChecked = true;
                                LoadMyPlayLists();
                            }
                            break;
                    }
                }
            }
        }

        private void TracksRadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb)
            {
                string buttonname = rb.Content.ToString();
                switch (buttonname)
                {
                    case "My Tracks":
                        LoadAllTracks();
                        break;
                    case "My Top Tracks":
                        LoadTopAllTracks();
                        break;
                }
                TrackSearchBar.Text = "";
            }
        }

        #region Searchs
        private void TrackSearchBar_SearchStarted(
            object sender,
            HandyControl.Data.FunctionEventArgs<string> e
        )
        {
            SearchStarted("track");
        }

        private void ArtistSearchBar_SearchStarted(
            object sender,
            HandyControl.Data.FunctionEventArgs<string> e
        )
        {
            SearchStarted("artist");
            MyTopArtists.IsChecked = false;
            SearchedArtist.IsChecked = true;
        }

        private void PlayListSearchBar_SearchStarted(
            object sender,
            HandyControl.Data.FunctionEventArgs<string> e
        )
        {
            SearchStarted("playlist");
            MyPlayListsRadioButton.IsChecked = false;
            SearchedPlayListsRadioButton.IsChecked = true;
        }

        private async void SearchStarted(string type)
        {
            if (!await SpotifyAuth.GrantAccess())
                return;

            CancellationService.Reset();
            var cancellationToken = CancellationService.Token;

            try
            {
                switch (type)
                {
                    case "track":
                        TracksListBox.Items.Clear();
                        LoadingPanel.Visibility = Visibility.Visible;

                        var searchTracks = await Requests.SearchRequests.Search<Track>(
                            Properties.Access.Default.AccessToken,
                            TrackSearchBar.Text,
                            "track",
                            cancellationToken
                        );
                        searchTracks = searchTracks.Where(p => p != null).ToList();
                        Dispatcher.Invoke(() =>
                        {
                            TracksListBox.Items.Clear();
                            MyTracks.IsChecked = false;
                            MyTopTracks.IsChecked = false;
                            foreach (var s in searchTracks)
                            {
                                TrackCard card = new TrackCard();
                                card.Name.Text = s.Name ?? "";
                                card.Artist.Text = s.Artists.FirstOrDefault().Name ?? "";
                                card.Album.Text = s.Album.Name ?? "";
                                card.TrackUri = s.Uri;
                                card.TrackId = s.Id;
                                if (s.Album.Images.Count != 0)
                                {
                                    card.Cover.Source =
                                        new System.Windows.Media.Imaging.BitmapImage(
                                            new Uri(s.Album.Images.FirstOrDefault().Url)
                                        );
                                }
                                TracksListBox.Items.Add(card);
                            }
                        });
                        break;
                    case "artist":
                        ArtistListBox.Items.Clear();
                        LoadingPanel.Visibility = Visibility.Visible;
                        var searchArtists = await Requests.SearchRequests.Search<Artist>(
                            Properties.Access.Default.AccessToken,
                            ArtistSearchBar.Text,
                            "artist",
                            cancellationToken
                        );
                        searchArtists = searchArtists.Where(p => p != null).ToList();
                        Dispatcher.Invoke(() =>
                        {
                            foreach (var s in searchArtists)
                            {
                                ArtistCard card = new ArtistCard();
                                card.Name.Text = s.Name;
                                if (s.Images.Count != 0)
                                {
                                    card.Cover.Source =
                                        new System.Windows.Media.Imaging.BitmapImage(
                                            new Uri(s.Images.FirstOrDefault().Url)
                                        );
                                }

                                ArtistListBox.Items.Add(card);
                            }
                        });
                        break;
                    case "playlist":
                        MyPlayLists.Items.Clear();
                        LoadingPanel.Visibility = Visibility.Visible;
                        var searchPlayLists = await Requests.SearchRequests.Search<Playlist>(
                            Properties.Access.Default.AccessToken,
                            PlayListSearchBar.Text,
                            "playlist",
                            cancellationToken
                        );

                        searchPlayLists = searchPlayLists.Where(p => p != null).ToList();

                        Dispatcher.Invoke(() =>
                        {
                            foreach (var s in searchPlayLists)
                            {
                                MyPlayListsCard card = new MyPlayListsCard();
                                card.PlayListName.Text = s.Name ?? "";
                                card.Owner.Text = s.Owner.DisplayName ?? "";
                                if (s.Images.Count != 0)
                                {
                                    card.Cover.Source =
                                        new System.Windows.Media.Imaging.BitmapImage(
                                            new Uri(s.Images.FirstOrDefault().Url)
                                        );
                                }
                                card.NumberOfTracks.Text = s.TrackInfo.Total.ToString() + " Songs";
                                MyPlayLists.Items.Add(card);
                            }
                        });
                        break;
                }
                if (type == "track") { }
                else { }
            }
            catch (OperationCanceledException oce)
            {
                if (cancellationToken.IsCancellationRequested)
                    Growl.Info("İşlem kullanıcı tarafından iptal edildi.");
                else
                    Growl.Warning(
                        "İstek zaman aşımına uğradı veya dışarıdan bir iptal oldu: " + oce.Message
                    );
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message);
            }
            finally
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
            }
        }
        #endregion
        private void ArtistsRadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb)
            {
                string buttonname = rb.Content.ToString();
                switch (buttonname)
                {
                    case "My Top Artists":
                        LoadTopArtists();
                        break;
                }
                ArtistSearchBar.Text = "";
            }
        }

        private void PlaylistRadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb)
            {
                string buttonname = rb.Content.ToString();
                switch (buttonname)
                {
                    case "My Play Lists":
                        LoadMyPlayLists();
                        break;
                }
                PlayListSearchBar.Text = "";
            }
        }
    }
}
