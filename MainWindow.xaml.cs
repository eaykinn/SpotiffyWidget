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
using HandyControl.Tools.Extension;
using SpotiffyWidget.Cards;
using SpotiffyWidget.Helpers;
using SpotiffyWidget.Models;
using SpotiffyWidget.Pages;
using SpotiffyWidget.Requests;
using TabControl = HandyControl.Controls.TabControl;
using TabItem = HandyControl.Controls.TabItem;

namespace SpotiffyWidget
{
    public partial class MainWindow
    {
        private bool _tracksOnLoad;
        private bool _playlistsOnLoad;

        private bool _isTracksCompactView = false;
        private bool _playlistsCompactView = false;

        public MainWindow()
        {
            InitializeComponent();
            _tracksOnLoad = true;

            _playlistsOnLoad = true;
        }

        #region Change Theme
        private void ButtonClick(object sender, RoutedEventArgs e) => CheckAccess();

        private void ButtonSkins_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button)
            {
                PopupConfig.IsOpen = false; // Close popup after a selection
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

                    picker.Confirmed += delegate
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
                int indx = -1;
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
                        indx += 1;
                        TrackCard card = new TrackCard();
                        card.Name.Content = s.Track.Name;
                        card.Artist.Content = s.Track.Artists.FirstOrDefault().Name;
                        card.Album.Content = s.Track.Album.Name;
                        card.TrackUri = s.Track.Uri;
                        card.TrackId = s.Track.Id;
                        card.IsTrackSaved = true;
                        card.LikeButton.IsChecked = true;
                        card.Cover.Source = new System.Windows.Media.Imaging.BitmapImage(
                            new Uri(s.Track.Album.Images.FirstOrDefault().Url)
                        );
                        var listboxItem = new ListBoxItem();
                        listboxItem.Margin = new Thickness(0, 2, 0, 2);
                        listboxItem.Padding = new Thickness(0, 0, 0, 0);
                        listboxItem.Content = card;
                        TracksListBox.Items.Add(listboxItem);
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
                        card.PlayListName.Content = s.Name;
                        card.Owner.Content = s.Owner.DisplayName;
                        card.Cover.Source = new System.Windows.Media.Imaging.BitmapImage(
                            new Uri(s.Images.FirstOrDefault().Url)
                        );
                        card.NumberOfTracks.Content = s.TrackInfo.Total.ToString() + " Songs";

                        var listboxItem = new ListBoxItem();
                        listboxItem.Margin = new Thickness(0, 2, 0, 2);
                        listboxItem.Padding = new Thickness(0, 0, 0, 0);
                        listboxItem.Content = card;

                        MyPlayLists.Items.Add(listboxItem);
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
                        card.Name.Content = s.Name;
                        card.Artist.Content = s.Artists.FirstOrDefault().Name;
                        card.Album.Content = s.Album.Name;
                        card.TrackUri = s.Uri;
                        card.TrackId = s.Id;
                        card.LikeButton.IsChecked = true;
                        card.IsTrackSaved = true;
                        card.Cover.Source = new System.Windows.Media.Imaging.BitmapImage(
                            new Uri(s.Album.Images.FirstOrDefault().Url)
                        );

                        var listboxItem = new ListBoxItem();
                        listboxItem.Margin = new Thickness(0, 2, 0, 2);
                        listboxItem.Padding = new Thickness(0, 0, 0, 0);
                        listboxItem.Content = card;

                        TracksListBox.Items.Add(listboxItem);
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

                        case "PlayLists":
                            if (_playlistsOnLoad)
                            {
                                _playlistsOnLoad = false;
                                PlayListSearchBar.Text = "";
                                MyPlayListsRadioButton.IsChecked = true;
                                LoadMyPlayLists();
                            }
                            break;
                        case "Artists":
                        {
                            MainArtistsFrame.Navigate(new ArtistsPage());

                            break;
                        }
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
                        int indx = -1;
                        TracksListBox.Items.Clear();
                        LoadingPanel.Visibility = Visibility.Visible;

                        var searchTracks = await Requests.SearchRequests.Search<Track>(
                            Properties.Access.Default.AccessToken,
                            TrackSearchBar.Text,
                            "track",
                            cancellationToken
                        );
                        searchTracks = searchTracks.Where(p => p != null).ToList();

                        string ids = string.Join(",", searchTracks.Select(t => t.Id));

                        var tracksSaved = await TracksRequests.CheckTracksIsSaved(
                            Properties.Access.Default.AccessToken,
                            ids,
                            cancellationToken
                        );

                        Dispatcher.Invoke(() =>
                        {
                            TracksListBox.Items.Clear();
                            MyTracks.IsChecked = false;
                            MyTopTracks.IsChecked = false;
                            foreach (var s in searchTracks)
                            {
                                indx += 1;
                                TrackCard card = new TrackCard();
                                card.Name.Content = s.Name ?? "";
                                card.Artist.Content = s.Artists.FirstOrDefault().Name ?? "";
                                card.Album.Content = s.Album.Name ?? "";
                                card.TrackUri = s.Uri;
                                card.TrackId = s.Id;
                                card.IsTrackSaved = tracksSaved[indx];
                                card.LikeButton.IsChecked = tracksSaved[indx];
                                if (s.Album.Images.Count != 0)
                                {
                                    card.Cover.Source =
                                        new System.Windows.Media.Imaging.BitmapImage(
                                            new Uri(s.Album.Images.FirstOrDefault().Url)
                                        );
                                }

                                var listboxItem = new ListBoxItem();
                                listboxItem.Margin = new Thickness(0, 2, 0, 2);
                                listboxItem.Padding = new Thickness(0, 0, 0, 0);
                                listboxItem.Content = card;

                                TracksListBox.Items.Add(listboxItem);
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
                                card.PlayListName.Content = s.Name ?? "";
                                card.Owner.Content = s.Owner.DisplayName ?? "";
                                if (s.Images.Count != 0)
                                {
                                    card.Cover.Source =
                                        new System.Windows.Media.Imaging.BitmapImage(
                                            new Uri(s.Images.FirstOrDefault().Url)
                                        );
                                }
                                card.NumberOfTracks.Content =
                                    s.TrackInfo.Total.ToString() + " Songs";

                                var listboxItem = new ListBoxItem();
                                listboxItem.Margin = new Thickness(0, 2, 0, 2);
                                listboxItem.Padding = new Thickness(0, 0, 0, 0);
                                listboxItem.Content = card;

                                MyPlayLists.Items.Add(listboxItem);
                            }
                        });
                        break;
                }
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

        private async void TracksDoubleClick(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        )
        {
            var tracksListBoxItem = TracksListBox.SelectedItem as ListBoxItem;

            if (tracksListBoxItem?.Content is TrackCard selectedCard)
            {
                if (!string.IsNullOrEmpty(selectedCard.TrackUri))
                {
                    string[] trackUris = new string[] { selectedCard.TrackUri };
                    await PlayTrack(trackUris);
                }
            }
        }

        private async Task PlayTrack(string[] trackUris)
        {
            if (!await SpotifyAuth.GrantAccess())
                return;

            if (!await SpotifyAuth.CheckDevice())
                return;

            var body = new { uris = trackUris };

            CancellationService.Reset();
            var cancellationToken = CancellationService.Token;

            await PlayerRequests.Play(
                Properties.Access.Default.AccessToken,
                body,
                cancellationToken
            );

            await PlayerCard.GetPlayBackStateAsync();
        }

        private void ChangeView_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || !(button.Tag is string targetListName))
                return;

            bool isCompact;
            string targetState;

            // Determine which view is being toggled and set the new state
            switch (targetListName)
            {
                case "TracksListBox":
                    _isTracksCompactView = !_isTracksCompactView;
                    isCompact = _isTracksCompactView;
                    break;
                case "MyPlayLists":
                    _playlistsCompactView = !_playlistsCompactView;
                    isCompact = _playlistsCompactView;
                    break;
                default:
                    return; // Unknown target
            }

            targetState = isCompact ? "CompactView" : "NormalView";

            // Find the ListBox control by its name
            if (this.FindName(targetListName) is ListBox targetListBox)
            {
                // Iterate through items to find ListBoxItems and change their visual state
                for (int i = 0; i < targetListBox.Items.Count; i++)
                {
                    if (
                        targetListBox.ItemContainerGenerator.ContainerFromIndex(i)
                        is ListBoxItem item
                    )
                    {
                        // Find the root element of the template where the VisualStates are defined.
                        var templateRoot = item.Template.FindName("Bd", item) as FrameworkElement;
                        if (templateRoot != null)
                            // Use GoToElementState to apply the state to the specific element within the item's template.
                            VisualStateManager.GoToElementState(templateRoot, targetState, true);
                    }
                }
            }
        }

        private void ChangeTheme(object sender, RoutedEventArgs e)
        {
            PopupConfig.IsOpen = !PopupConfig.IsOpen;
        }

        private async void TrackPlayButton_Click(object sender, RoutedEventArgs e)
        {
            var tracksList = new System.Collections.Generic.List<string>();

            foreach (var item in TracksListBox.Items)
            {
                if (
                    item is ListBoxItem listBoxItem
                    && listBoxItem.Content is TrackCard selectedCard
                )
                {
                    if (!string.IsNullOrEmpty(selectedCard.TrackUri))
                    {
                        tracksList.Add(selectedCard.TrackUri);
                    }
                }
            }

            if (tracksList.Count > 0)
            {
                await PlayTrack(tracksList.ToArray());
            }
        }
    }
}
