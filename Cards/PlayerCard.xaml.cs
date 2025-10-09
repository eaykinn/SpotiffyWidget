using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HandyControl.Controls;
using SpotiffyWidget.Helpers;
using SpotiffyWidget.Models;
using SpotiffyWidget.Requests;
using static SpotiffyWidget.Helpers.CancellationService;
using Path = System.IO.Path;

namespace SpotiffyWidget.Cards
{
    /// <summary>
    /// Interaction logic for TrackCard.xaml
    /// </summary>
    public partial class PlayerCard : UserControl
    {
        public PlayerCard()
        {
            InitializeComponent();

            NPSMLibFunctions.GetNowPlayingInfo();
            Cover.Source = NPSMLibFunctions.Image;
            SongName.Text = NPSMLibFunctions.SongName;
            ArtistName.Text = NPSMLibFunctions.ArtistName;
            AlbumName.Text = NPSMLibFunctions.AlbumName;
            MaxTime.Text = NPSMLibFunctions.MaxTime;
            CurrentTime.Text = NPSMLibFunctions.CurrentTime;

            PlayerSlider.Interval = 1;
            PlayerSlider.Maximum = NPSMLibFunctions.MaxSeconds;
            PlayerSlider.Value = (double)NPSMLibFunctions.CurrentSecond;
        }

        private async void PlayPause()
        {
            if (!await SpotifyApiHelper.GrantAccess())
                return;

            CancellationService.Reset();
            var cancellationToken = CancellationService.Token;

            await Task.Run(async () =>
            {
                var devices = await Requests.PlayerRequests.GetDevices(
                    Properties.Access.Default.AccessToken,
                    cancellationToken
                );

                if (devices.DeviceList.Count == 0)
                {
                    HandyControl.Controls.MessageBox.Show(
                        "No active device found. Please open Spotify on one of your devices.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return;
                }

                var playBackState = await Requests.PlayerRequests.GetPlayBackState(
                    Properties.Access.Default.AccessToken,
                    cancellationToken
                );

                if (playBackState.IsPlaying)
                {
                    await Requests.PlayerRequests.Pause(
                        Properties.Access.Default.AccessToken,
                        null,
                        cancellationToken
                    );
                    return;
                }
                else
                {
                    Console.WriteLine(devices);

                    await Requests.PlayerRequests.Play(
                        Properties.Access.Default.AccessToken,
                        null,
                        cancellationToken
                    );
                }
            });
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            PlayPause();
        }
    }
}
