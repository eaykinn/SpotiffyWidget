using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using SpotiffyWidget.Cards;
using SpotiffyWidget.Helpers;
using SpotiffyWidget.Models;
using SpotiffyWidget.Requests;

namespace SpotiffyWidget.Pages
{
    /// <summary>
    /// Interaction logic for TracksPage.xaml
    /// </summary>
    public partial class TracksPage : Page
    {
        private bool _isTracksCompactView = false;
        private bool IsFromAlbum;

        public TracksPage(string id, string trackImageUri, bool IsAlbum)
        {
            IsFromAlbum = IsAlbum;
            InitializeComponent();
            LoadTracks(id, trackImageUri, IsAlbum);
        }

        private async void LoadTracks(string id, string trackImageUri, bool IsAlbum)
        {
            // TODO: Implement track loading logic
            if (!await SpotifyAuth.GrantAccess())
                return;

            CancellationService.Reset();
            var cancellationToken = CancellationService.Token;
            var tracks = new List<Track>();
            if (IsAlbum)
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
            }
            else
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
                    .Take(50)
                    .ToList();

                AlbumName.Content = playlist.Name;
                ArtistName.Content = playlist.Owner.DisplayName;
            }

            string ids = string.Join(",", tracks.Select(t => t.Id));

            var tracksSaved = await TracksRequests.CheckTracksIsSaved(
                Properties.Access.Default.AccessToken,
                ids,
                cancellationToken
            );

            TrackCount.Content = tracks.Count() + " Songs";
            BitmapImage imageSource = new BitmapImage(new Uri(trackImageUri));

            foreach (var s in tracks)
            {
                TrackCard card = new TrackCard();

                card.Name.Content = s.Name;

                if (IsAlbum)
                {
                    card.Artist.Content =
                        $"{(int)s.DurationMs / 60000}:{(s.DurationMs % 60000) / 1000:D2}";
                }
                else
                {
                    card.Artist.Content = s.Artists.FirstOrDefault().Name;

                    card.Album.Content =
                        $"{(int)s.DurationMs / 60000}:{(s.DurationMs % 60000) / 1000:D2}";
                }

                card.TrackUri = s.Uri;
                card.TrackId = s.Id;
                card.Cover.Source = imageSource;
                card.IsTrackSaved = tracksSaved[tracks.IndexOf(s)] ? true : false;
                card.LikeButton.IsChecked = tracksSaved[tracks.IndexOf(s)] ? true : false;

                //card.LikeButton.IsChecked = true;
                //card.IsTrackSaved = true;

                var listboxItem = new ListBoxItem();
                listboxItem.Margin = new Thickness(0, 2, 0, 2);
                listboxItem.Padding = new Thickness(0, 0, 0, 0);
                listboxItem.Content = card;

                TracksListBox.Items.Add(listboxItem);
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

        private void TracksDoubleClick(object sender, MouseButtonEventArgs e) { }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var mw = Application.Current.MainWindow as MainWindow;
            if (mw != null)
            {
                if (IsFromAlbum)
                {
                    if (mw.MainArtistsFrame.CanGoBack)
                        mw.MainArtistsFrame.GoBack();
                }
                else
                {
                    if (mw.MainPlayListsFrame.CanGoBack)
                        mw.MainPlayListsFrame.GoBack();
                }
            }
        }
    }
}
