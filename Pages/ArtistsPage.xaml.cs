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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HandyControl.Controls;
using SpotiffyWidget.Cards;
using SpotiffyWidget.Helpers;
using SpotiffyWidget.Models;
using SpotiffyWidget.Requests;
using SpotiffyWidget.SpotifyEndPoints;

namespace SpotiffyWidget.Pages
{
    /// <summary>
    /// Interaction logic for ArtistsPage.xaml
    /// </summary>
    public partial class ArtistsPage : Page
    {
        private bool _tracksOnLoad;
        private bool _artistsOnLoad;
        private bool _playlistsOnLoad;

        private bool _isTracksCompactView = false;
        private bool _artistsCompactView = false;
        private bool _playlistsCompactView = false;

        // Remove or comment out the following line in the ArtistsPage constructor:
        // this.NavigationCacheMode = System.Windows.Navigation.NavigationCacheMode.Required;

        // Fixed constructor:
        public ArtistsPage()
        {
            InitializeComponent();

            LoadTopArtists();
        }

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
                        card.Name.Content = s.Name;
                        card.ArtistId = s.Id;
                        card.Cover.Source = new System.Windows.Media.Imaging.BitmapImage(
                            new Uri(s.Images.FirstOrDefault().Url)
                        );

                        var listboxItem = new ListBoxItem();
                        listboxItem.Margin = new Thickness(0, 2, 0, 2);
                        listboxItem.Padding = new Thickness(0, 0, 0, 0);
                        listboxItem.Content = card;

                        ArtistListBox.Items.Add(listboxItem);
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
                MyTopArtists.IsChecked = true;
            }
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
                    case "artist":
                        ArtistListBox.Items.Clear();
                        LoadingPanel.Visibility = Visibility.Visible;
                        var searchArtists = await Requests.SearchRequests.Search<Models.Artist>(
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
                                card.Name.Content = s.Name;
                                card.ArtistId = s.Id;
                                if (s.Images.Count != 0)
                                {
                                    card.Cover.Source =
                                        new System.Windows.Media.Imaging.BitmapImage(
                                            new Uri(s.Images.FirstOrDefault().Url)
                                        );
                                }

                                var listboxItem = new ListBoxItem();
                                listboxItem.Margin = new Thickness(0, 2, 0, 2);
                                listboxItem.Padding = new Thickness(0, 0, 0, 0);
                                listboxItem.Content = card;

                                ArtistListBox.Items.Add(listboxItem);
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

        private void ChangeView_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || !(button.Tag is string targetListName))
                return;

            bool isCompact;
            string targetState;

            // Determine which view is being toggled and set the new state
            switch (targetListName)
            {
                case "ArtistListBox":
                    _artistsCompactView = !_artistsCompactView;
                    isCompact = _artistsCompactView;
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
    }
}
