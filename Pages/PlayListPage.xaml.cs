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
    /// Interaction logic for PlayListPage.xaml
    /// </summary>
    public partial class PlayListPage : Page
    {
        private bool _playlistsCompactView = false;

        public PlayListPage()
        {
            InitializeComponent();
            LoadMyPlayLists();
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

        private void PlayListSearchBar_SearchStarted(
            object sender,
            HandyControl.Data.FunctionEventArgs<string> e
        )
        {
            SearchStarted("playlist");
            MyPlayListsRadioButton.IsChecked = false;
            SearchedPlayListsRadioButton.IsChecked = true;
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
                        card.Type = "PlayList";
                        card.Id = s.Id;
                        card.ImageUri = s.Images.FirstOrDefault().Url;
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
                if (!cancellationToken.IsCancellationRequested)
                    Growl.Warning("Request timeout: " + oce.Message);
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message);
            }
            finally
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
                MyPlayListsRadioButton.IsChecked = true;
            }
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
                                card.Type = "PlayList";
                                card.Id = s.Id;
                                card.ImageUri = s.Images.FirstOrDefault().Url;
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
                if (!cancellationToken.IsCancellationRequested)
                    Growl.Warning("Request timeout: " + oce.Message);
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
    }
}
