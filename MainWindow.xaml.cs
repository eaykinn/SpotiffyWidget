using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HandyControl.Controls;
using HandyControl.Themes;
using HandyControl.Tools;
using SpotiffyWidget.Helpers;
using SpotiffyWidget.Models;

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

        public async Task<bool> GrantAccess()
        {
            if (Properties.Access.Default.AccessToken != "")
            {
                if (!await SpotifyAuth.CheckToken(Properties.Access.Default.AccessToken))
                {
                    string accessToken = await SpotifyAuth.RefreshAccessToken(
                        Properties.Access.Default.RefreshToken
                    );
                    if (accessToken == null)
                    {
                        Growl.Info("Could not refresh access token.");
                        return false;
                    }
                    return true;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                //token yok
                string authcode = SpotifyAuth.GetAuthCode();
                var accesstoken = await SpotifyAuth.GetAccessToken(authcode);
                if (accesstoken.Count == 0)
                {
                    Growl.Info("Could not get access token.");
                    return false;
                }
                return true;
            }
        }

        private async void AccessButton_Click(object sender, RoutedEventArgs e)
        {
            TracksListBox.Items.Clear();

            if (await GrantAccess())
            {
                var tracks = await Requests.ProfileRequests.GetTopTracksAsync(
                    Properties.Access.Default.AccessToken
                );

                foreach (var item in tracks)
                {
                    TracksListBox.Items.Add(item.Name);
                }

                var artists = await Requests.ProfileRequests.GetTopArtistsAsync(
                    Properties.Access.Default.AccessToken
                );

                foreach (var item in artists)
                {
                    TracksListBox.Items.Add(item.Name);
                }

                var searchArtists = await Requests.SearchRequests.Search<Artist>(
                    Properties.Access.Default.AccessToken,
                    "Imagine Dragons",
                    "artist"
                );

                foreach (var item in searchArtists)
                {
                    TracksListBox.Items.Add(item.Name);
                }
                TracksListBox.Items.Clear();
                var playlists = await Requests.ProfileRequests.GetUsersPlaylists(
                    Properties.Access.Default.AccessToken
                );

                foreach (var item in playlists)
                {
                    TracksListBox.Items.Add(item.Name);
                }
            }
        }
    }
}
