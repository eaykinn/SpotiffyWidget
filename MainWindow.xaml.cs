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
        public MainWindow()
        {
            InitializeComponent();
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
            if (!await SpotifyApiHelper.GrantAccess())
                return;

            CancellationService.Reset();
            var cancellationToken = CancellationService.Token;

            Cancel.IsEnabled = true;

            LoadingPanel.Visibility = Visibility.Visible;

            try
            {
                await Task.Run(async () =>
                {
                    // 1) Top tracks
                    /*var tracks = await Requests.ProfileRequests.GetTopTracksAsync(
                        Properties.Access.Default.AccessToken,
                        10,
                        cancellationToken
                    );
                    cancellationToken.ThrowIfCancellationRequested();
                    var trackNames = tracks.Select(t => t.Name).ToList();
                    */
                    // 2) Artists
                    var artists = await Requests.ProfileRequests.GetTopArtistsAsync(
                        Properties.Access.Default.AccessToken,
                        cancellationToken
                    );
                    cancellationToken.ThrowIfCancellationRequested();
                    var artistNames = artists.Select(a => a.Name).ToList();

                    Dispatcher.Invoke(() =>
                    {
                        TracksListBox.Items.Clear();
                        foreach (var s in artists)
                        {
                            ArtistCard card = new ArtistCard();
                            card.Name.Text = s.Name;

                            card.Cover.Source = new System.Windows.Media.Imaging.BitmapImage(
                                new Uri(s.Images.FirstOrDefault().Url)
                            );

                            TracksListBox.Items.Add(card);
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

                Cancel.IsEnabled = false;
            }
        }

        private async void LoadAllTracks()
        {
            if (!await SpotifyApiHelper.GrantAccess())
                return;

            CancellationService.Reset();
            var cancellationToken = CancellationService.Token;

            Cancel.IsEnabled = true;

            LoadingPanel.Visibility = Visibility.Visible;

            try
            {
                await Task.Run(async () =>
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
                        ArtistListBox.Items.Clear();
                        foreach (var s in allTracks)
                        {
                            TrackCard card = new TrackCard();
                            card.Name.Text = s.Track.Name;
                            card.Artist.Text = s.Track.Artists.FirstOrDefault().Name;
                            card.Album.Text = s.Track.Album.Name;
                            card.Cover.Source = new System.Windows.Media.Imaging.BitmapImage(
                                new Uri(s.Track.Album.Images.FirstOrDefault().Url)
                            );

                            ArtistListBox.Items.Add(card);
                        }
                    });
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

                Cancel.IsEnabled = false;
            }
        }

        private async void LoadMyPlayLists()
        {
            if (!await SpotifyApiHelper.GrantAccess())
                return;

            CancellationService.Reset();
            var cancellationToken = CancellationService.Token;

            Cancel.IsEnabled = true;

            LoadingPanel.Visibility = Visibility.Visible;

            try
            {
                await Task.Run(async () =>
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
                        MyPlayLists.Items.Clear();
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

                Cancel.IsEnabled = false;
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
                        case "My Tracks":
                            LoadAllTracks();
                            break;
                        case "My Top Artists":
                            LoadTopArtists();
                            break;
                        case "My PlayLists":
                            LoadMyPlayLists();
                            break;
                    }
                }
            }
        }
    }
}
