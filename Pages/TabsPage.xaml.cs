using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using HandyControl.Controls;
using HandyControl.Data;
using SpotiffyWidget.Cards;
using SpotiffyWidget.Helpers;
using SpotiffyWidget.Models;
using SpotiffyWidget.Properties;
using SpotiffyWidget.Requests;
using TabControl = HandyControl.Controls.TabControl;
using TabItem = HandyControl.Controls.TabItem;

namespace SpotiffyWidget.Pages;

/// <summary>
///     Interaction logic for TabsPage.xaml
/// </summary>
public partial class TabsPage : Page
{
    private bool _artistsOnLoad = true;

    private bool _isTracksCompactView;
    private bool _playlistsOnLoad = true; // Bunu ekle
    private bool _tracksOnLoad = true;

    public TabsPage()
    {
        InitializeComponent();
    }

    private void ButtonClick(object sender, RoutedEventArgs e)
    {
        CheckAccess();
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
            var indx = -1;
            // 5) All tracks
            var allTracks = await ProfileRequests.GetTracksAsync(
                Access.Default.AccessToken,
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
                    var card = new TrackCard();
                    card.Name.Content = s.Track.Name;
                    card.Artist.Content = s.Track.Artists.FirstOrDefault().Name;
                    card.Album.Content = s.Track.Album.Name;
                    card.TrackUri = s.Track.Uri;
                    card.TrackId = s.Track.Id;
                    card.IsTrackSaved = true;
                    card.LikeButton.IsChecked = true;
                    card.Cover.Source = new BitmapImage(
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
            var allTracks = await ProfileRequests.GetTopTracksAsync(
                Access.Default.AccessToken,
                10,
                cancellationToken
            );
            cancellationToken.ThrowIfCancellationRequested();

            // Tek seferde UI güncellemesi
            Dispatcher.Invoke(() =>
            {
                foreach (var s in allTracks)
                {
                    var card = new TrackCard();
                    card.Name.Content = s.Name;
                    card.Artist.Content = s.Artists.FirstOrDefault().Name;
                    card.Album.Content = s.Album.Name;
                    card.TrackUri = s.Uri;
                    card.TrackId = s.Id;
                    card.LikeButton.IsChecked = true;
                    card.IsTrackSaved = true;
                    card.Cover.Source = new BitmapImage(
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
                    {
                        if (_playlistsOnLoad) // Sadece ilk kez tıklandığında yükle
                        {
                            _playlistsOnLoad = false; // Bayrağı kapat
                            MainPlayListsFrame.Navigate(new PlayListPage());
                        }
                    }
                        break;
                    case "Artists":
                    {
                        if (_artistsOnLoad) // Sadece ilk kez tıklandığında yükle
                        {
                            _artistsOnLoad = false;
                            MainArtistsFrame.Navigate(new ArtistsPage());
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
            var buttonname = rb.Content.ToString();
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


    private async void TracksDoubleClick(
        object sender,
        MouseButtonEventArgs e
    )
    {
        var tracksListBoxItem = TracksListBox.SelectedItem as ListBoxItem;

        if (tracksListBoxItem?.Content is TrackCard selectedCard)
            if (!string.IsNullOrEmpty(selectedCard.TrackUri))
            {
                var trackUris = new[] { selectedCard.TrackUri };
                await PlayTrack(trackUris);
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
            Access.Default.AccessToken,
            body,
            cancellationToken
        );

        var mw = Application.Current.MainWindow as MainWindow;

        await mw.PlayerCard.GetPlayBackStateAsync();
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
        if (FindName(targetListName) is ListBox targetListBox)
            // Iterate through items to find ListBoxItems and change their visual state
            for (var i = 0; i < targetListBox.Items.Count; i++)
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

    private async void TrackPlayButton_Click(object sender, RoutedEventArgs e)
    {
        var tracksList = new List<string>();

        foreach (var item in TracksListBox.Items)
            if (
                item is ListBoxItem listBoxItem
                && listBoxItem.Content is TrackCard selectedCard
            )
                if (!string.IsNullOrEmpty(selectedCard.TrackUri))
                    tracksList.Add(selectedCard.TrackUri);

        if (tracksList.Count > 0) await PlayTrack(tracksList.ToArray());
    }

    #region Searchs

    private void TrackSearchBar_SearchStarted(
        object sender,
        FunctionEventArgs<string> e
    )
    {
        SearchStarted("track");
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
                    var indx = -1;
                    TracksListBox.Items.Clear();
                    LoadingPanel.Visibility = Visibility.Visible;

                    var searchTracks = await SearchRequests.Search<Track>(
                        Access.Default.AccessToken,
                        TrackSearchBar.Text,
                        "track",
                        cancellationToken
                    );
                    searchTracks = searchTracks.Where(p => p != null).ToList();

                    var ids = string.Join(",", searchTracks.Select(t => t.Id));

                    var tracksSaved = await TracksRequests.CheckTracksIsSaved(
                        Access.Default.AccessToken,
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
                            var card = new TrackCard();
                            card.Name.Content = s.Name ?? "";
                            card.Artist.Content = s.Artists.FirstOrDefault().Name ?? "";
                            card.Album.Content = s.Album.Name ?? "";
                            card.TrackUri = s.Uri;
                            card.TrackId = s.Id;
                            card.IsTrackSaved = tracksSaved[indx];
                            card.LikeButton.IsChecked = tracksSaved[indx];
                            if (s.Album.Images.Count != 0)
                                card.Cover.Source =
                                    new BitmapImage(
                                        new Uri(s.Album.Images.FirstOrDefault().Url)
                                    );

                            var listboxItem = new ListBoxItem();
                            listboxItem.Margin = new Thickness(0, 2, 0, 2);
                            listboxItem.Padding = new Thickness(0, 0, 0, 0);
                            listboxItem.Content = card;

                            TracksListBox.Items.Add(listboxItem);
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
}