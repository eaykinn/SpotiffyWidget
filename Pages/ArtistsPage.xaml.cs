using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using HandyControl.Controls;
using HandyControl.Data;
using SpotiffyWidget.Cards;
using SpotiffyWidget.Helpers;
using SpotiffyWidget.Models;
using SpotiffyWidget.Properties;
using SpotiffyWidget.Requests;

namespace SpotiffyWidget.Pages;

/// <summary>
///     Interaction logic for ArtistsPage.xaml
/// </summary>
public partial class ArtistsPage : Page
{
    private bool _artistsCompactView;

    public ArtistsPage()
    {
        InitializeComponent();

        LoadTopArtists();
    }

    private void ArtistsRadioButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb)
        {
            var buttonname = rb.Content.ToString();
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
            var artists = await ProfileRequests.GetTopArtistsAsync(
                Access.Default.AccessToken,
                cancellationToken
            );
            cancellationToken.ThrowIfCancellationRequested();
            var artistNames = artists.Select(a => a.Name).ToList();

            Dispatcher.Invoke(() =>
            {
                foreach (var s in artists)
                {
                    var card = new ArtistCard();
                    card.Name.Content = s.Name;
                    card.ArtistId = s.Id;
                    card.Cover.Source = new BitmapImage(new Uri(s.Images.FirstOrDefault().Url));

                    var listboxItem = new ListBoxItem();
                    listboxItem.Margin = new Thickness(0, 2, 0, 2);
                    listboxItem.Padding = new Thickness(0, 0, 0, 0);
                    listboxItem.Content = card;

                    ArtistListBox.Items.Add(listboxItem);
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
            MyTopArtists.IsChecked = true;
        }
    }

    private void ArtistSearchBar_SearchStarted(object sender, FunctionEventArgs<string> e)
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
                    var searchArtists = await SearchRequests.Search<Artist>(
                        Access.Default.AccessToken,
                        ArtistSearchBar.Text,
                        "artist",
                        cancellationToken
                    );
                    searchArtists = searchArtists.Where(p => p != null).ToList();
                    Dispatcher.Invoke(() =>
                    {
                        foreach (var s in searchArtists)
                        {
                            var card = new ArtistCard();
                            card.Name.Content = s.Name;
                            card.ArtistId = s.Id;
                            if (s.Images.Count != 0)
                                card.Cover.Source = new BitmapImage(
                                    new Uri(s.Images.FirstOrDefault().Url)
                                );

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
        if (FindName(targetListName) is ListBox targetListBox)
            // Iterate through items to find ListBoxItems and change their visual state
            for (var i = 0; i < targetListBox.Items.Count; i++)
                if (targetListBox.ItemContainerGenerator.ContainerFromIndex(i) is ListBoxItem item)
                {
                    // Find the root element of the template where the VisualStates are defined.
                    var templateRoot = item.Template.FindName("Bd", item) as FrameworkElement;
                    if (templateRoot != null)
                        // Use GoToElementState to apply the state to the specific element within the item's template.
                        VisualStateManager.GoToElementState(templateRoot, targetState, true);
                }
    }
}
