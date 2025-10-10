using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
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
        DispatcherTimer uiTimer = new DispatcherTimer();

        public PlayerCard()
        {
            InitializeComponent();

            uiTimer = new DispatcherTimer();
            uiTimer.Interval = TimeSpan.FromMilliseconds(100);
            uiTimer.Tick += UpdateSongInfo;
            uiTimer.Start();
        }

        private void UpdateSongInfo(object? sender, EventArgs e)
        {
            NPSMLibFunctions.UpdatePlayback();
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
            if (!await SpotifyAuth.GrantAccess())
                return;

            if (!await SpotifyAuth.CheckDevice())
                return;

            Reset();
            var cancellationToken = Token;

            var playBackState = await PlayerRequests.GetPlayBackState(
                Properties.Access.Default.AccessToken,
                cancellationToken
            );

            if (playBackState.IsPlaying)
            {
                await PlayerRequests.Pause(
                    Properties.Access.Default.AccessToken,
                    null,
                    cancellationToken
                );
                uiTimer.Stop();
                return;
            }
            else
            {
                await PlayerRequests.Play(
                    Properties.Access.Default.AccessToken,
                    null,
                    cancellationToken
                );
                uiTimer.Start();
                return;
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            PlayPause();
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await SpotifyAuth.GrantAccess())
                return;

            if (!await SpotifyAuth.CheckDevice())
                return;

            Reset();
            var cancellationToken = Token;
            bool ok = await PlayerRequests.Next(
                Properties.Access.Default.AccessToken,
                cancellationToken
            );
        }

        private async void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await SpotifyAuth.GrantAccess())
                return;

            if (!await SpotifyAuth.CheckDevice())
                return;

            Reset();
            var cancellationToken = Token;

            bool ok = await PlayerRequests.Previous(
                Properties.Access.Default.AccessToken,
                cancellationToken
            );
        }
    }
}
