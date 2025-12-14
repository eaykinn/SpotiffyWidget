using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using SpotiffyWidget.Cards;
using SpotiffyWidget.Helpers;
using SpotiffyWidget.Models;
using SpotiffyWidget.Properties;
using SpotiffyWidget.Requests;

namespace SpotiffyWidget.Pages
{
    /// <summary>
    /// Interaction logic for TracksPage.xaml
    /// </summary>
    public partial class TracksPage : Page
    {
        private bool _isTracksCompactView = false;
        private int IsFromWhere;
        private List<Track> _allTracks = new List<Track>();
        private Dictionary<string, bool> _savedTracksCache = new Dictionary<string, bool>();
        private int _loadedCount = 0;
        private const int BATCH_SIZE = 20; // Her seferinde 20 şarkı yükle
        private bool _isLoading = false;
        private string _trackImageUri = "";

        public TracksPage(string id, string trackImageUri, int IsFromWhere)
        {
            this.IsFromWhere = IsFromWhere;
            _trackImageUri = trackImageUri;
            InitializeComponent();
            LoadTracks(id, trackImageUri, IsFromWhere);
        }

        private async void LoadTracks(string id, string trackImageUri, int IsFromWhere)
        {
            LoadingPanel.Visibility = Visibility.Visible;

            if (!await SpotifyAuth.GrantAccess())
                return;

            CancellationService.Reset();
            var cancellationToken = CancellationService.Token;
            var tracks = new List<Track>();

            try
            {
                switch (IsFromWhere)
                {
                    case 0: // Album
                    {
                        tracks = await AlbumsRequests.GetAlbumTracks(
                            Properties.Access.Default.AccessToken,
                            id,
                            cancellationToken
                        );

                        var album = await AlbumsRequests.GetAlbum(
                            Properties.Access.Default.AccessToken,
                            id,
                            cancellationToken
                        );

                        AlbumName.Content = album.Name;
                        ArtistName.Content = album.Artists.FirstOrDefault().Name;
                        break;
                    }
                    case 1: // Playlist
                    {
                        var Profiletracks = await PlayListsRequests.GetPlayListTracks(
                            Properties.Access.Default.AccessToken,
                            id,
                            cancellationToken
                        );
                        var playlist = await PlayListsRequests.GetPlayList(
                            Properties.Access.Default.AccessToken,
                            id,
                            cancellationToken
                        );

                        tracks = Profiletracks
                            .Where(x => x.Track != null)
                            .Select(pt => pt.Track)
                            .ToList();

                        AlbumName.Content = playlist.Name;
                        ArtistName.Content = playlist.Owner.DisplayName;
                        break;
                    }
                    case 2: // Queue - en kritik durum
                    {
                        var queue = await PlayerRequests.GetUserQueue(
                            Access.Default.AccessToken,
                            cancellationToken
                        );

                        if (queue.QueueTrack == null)
                            return;

                        tracks = queue.QueueTrack.Where(x => x.Id != null).ToList();

                        AlbumName.Content = "Queue";
                        ArtistName.Content = "User's";
                        break;
                    }
                    case 3: // Artist Top Tracks
                    {
                        var artist = await ArtistsRequests.GetArtist(
                            Access.Default.AccessToken,
                            id,
                            cancellationToken
                        );

                        var queue = await ArtistsRequests.GetArtistTopTracks(
                            Access.Default.AccessToken,
                            id,
                            cancellationToken
                        );

                        if (queue == null)
                        {
                            LoadingPanel.Visibility = Visibility.Hidden;
                            return;
                        }
                        tracks = queue.Where(x => x.Id != null).ToList();

                        AlbumName.Content = "Top Tracks";
                        ArtistName.Content = artist.Name + "'s";
                        break;
                    }
                }

                _allTracks = tracks;
                TrackCount.Content = tracks.Count + " Songs";

                // İlk batch'i yükle
                await LoadNextBatch(trackImageUri);
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error($"Error loading tracks: {ex.Message}");
            }
            finally
            {
                LoadingPanel.Visibility = Visibility.Hidden;
            }
        }

        private async Task LoadNextBatch(string trackImageUri)
        {
            if (_isLoading || _loadedCount >= _allTracks.Count)
                return;

            _isLoading = true;

            try
            {
                // Yüklenecek şarkıların batch'ini al
                var tracksToLoad = _allTracks
                    .Skip(_loadedCount)
                    .Take(BATCH_SIZE)
                    .ToList();

                if (tracksToLoad.Count == 0)
                {
                    _isLoading = false;
                    return;
                }

                // Saved status kontrolü - batch halinde
                CancellationService.Reset();
                var cancellationToken = CancellationService.Token;

                string ids = string.Join(",", tracksToLoad.Select(t => t.Id));
                var tracksSaved = await TracksRequests.CheckTracksIsSaved(
                    Properties.Access.Default.AccessToken,
                    ids,
                    cancellationToken
                );

                // Cache'e ekle
                for (int i = 0; i < tracksToLoad.Count; i++)
                {
                    _savedTracksCache[tracksToLoad[i].Id] = tracksSaved[i];
                }

                // UI'a ekle (Dispatcher kullanarak)
                await Dispatcher.InvokeAsync(() =>
                {
                    foreach (var s in tracksToLoad)
                    {
                        TrackCard card = new TrackCard();

                        card.Name.Content = s.Name;

                        if (IsFromWhere == 1)
                        {
                            card.Artist.Content =
                                $"{(int)s.DurationMs / 60000}:{(s.DurationMs % 60000) / 1000:D2}";
                        }
                        else
                        {
                            card.Artist.Content =
                                s.Artists?.FirstOrDefault()?.Name ?? "";

                            card.Album.Content =
                                $"{(int)s.DurationMs / 60000}:{(s.DurationMs % 60000) / 1000:D2}";
                        }

                        card.TrackUri = s.Uri;
                        card.TrackId = s.Id;

                        // Cover image
                        if (trackImageUri == "" && s.Album != null)
                        {
                            BitmapImage img = new BitmapImage(
                                new Uri(s.Album?.Images.FirstOrDefault()?.Url)
                            );
                            card.Cover.Source = img;
                        }
                        else if (trackImageUri.Length > 0)
                        {
                            BitmapImage imageSource = new BitmapImage(new Uri(trackImageUri));
                            card.Cover.Source = imageSource;
                        }

                        // Saved status
                        bool isSaved = _savedTracksCache.ContainsKey(s.Id)
                            ? _savedTracksCache[s.Id]
                            : false;
                        card.IsTrackSaved = isSaved;
                        card.LikeButton.IsChecked = isSaved;

                        var listboxItem = new ListBoxItem();
                        listboxItem.Margin = new Thickness(0, 2, 0, 2);
                        listboxItem.Padding = new Thickness(0, 0, 0, 0);
                        listboxItem.Content = card;

                        TracksListBox.Items.Add(listboxItem);
                    }
                }, System.Windows.Threading.DispatcherPriority.Background);

                _loadedCount += tracksToLoad.Count;
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Warning($"Batch loading error: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async void TracksListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Scroll sona yaklaştığında yeni batch yükle
            var scrollViewer = e.OriginalSource as ScrollViewer;
            if (scrollViewer != null)
            {
                // Son %20'ye gelince yeni batch yükle
                double threshold = scrollViewer.ScrollableHeight * 0.8;
                if (scrollViewer.VerticalOffset >= threshold && !_isLoading)
                {
                    await LoadNextBatch(_trackImageUri);
                }
            }
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

        private void TrackSearchBar_SearchStarted(
            object sender,
            HandyControl.Data.FunctionEventArgs<string> e
        )
        {
            string searchText = TrackSearchBar.Text.ToLower();

            foreach (ListBoxItem item in TracksListBox.Items)
            {
                if (item.Content is TrackCard t)
                {
                    var nameLabel = t.FindName("Name") as Label;
                    if (nameLabel != null)
                    {
                        string name = nameLabel.Content.ToString().ToLower();

                        // Eşleşme varsa göster, yoksa gizle
                        item.Visibility = name.Contains(searchText)
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                    }
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

        private async void TracksDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var tracksListBoxItem = TracksListBox.SelectedItem as ListBoxItem;

            if (tracksListBoxItem?.Content is TrackCard selectedCard)
                if (!string.IsNullOrEmpty(selectedCard.TrackUri))
                {
                    var trackUris = new[] { selectedCard.TrackUri };
                    await PlayTrack(trackUris);
                }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mw)
            {
                if (mw.TabsFrame.CanGoBack)
                    mw.TabsFrame.GoBack();
            }
        }
    }
}
