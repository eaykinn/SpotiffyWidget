using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using SpotiffyWidget.Helpers;
using SpotiffyWidget.Models;
using SpotiffyWidget.Requests;

namespace SpotiffyWidget.Cards
{
    /// <summary>
    /// Interaction logic for TrackCard.xaml
    /// </summary>
    public partial class TrackCard : UserControl
    {
        public string TrackUri { get; set; }
        public string TrackId { get; set; }

        public TrackCard()
        {
            InitializeComponent();
        }

        private async Task PlayTrack(string trackUri)
        {
            if (!await SpotifyAuth.GrantAccess())
                return;

            if (!await SpotifyAuth.CheckDevice())
                return;

            var body = new { uris = new string[] { trackUri } };

            CancellationService.Reset();
            var cancellationToken = CancellationService.Token;

            await PlayerRequests.Play(
                Properties.Access.Default.AccessToken,
                body,
                cancellationToken
            );
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            await PlayTrack(this.TrackUri);
        }

        private async Task AddQueue(string trackUri)
        {
            if (!await SpotifyAuth.GrantAccess())
                return;

            if (!await SpotifyAuth.CheckDevice())
                return;

            CancellationService.Reset();
            var cancellationToken = CancellationService.Token;

            await PlayerRequests.AddQueue(
                Properties.Access.Default.AccessToken,
                trackUri,
                cancellationToken
            );
        }

        private async void AddQueueButton_Click(object sender, RoutedEventArgs e)
        {
            await AddQueue(this.TrackUri);
        }

        private async Task LikeSong(string TrackId)
        {
            if (!await SpotifyAuth.GrantAccess())
                return;

            if (!await SpotifyAuth.CheckDevice())
                return;

            CancellationService.Reset();
            var cancellationToken = CancellationService.Token;
            var body = new { ids = new string[] { TrackId } };
            await TracksRequests.LikeSong(
                Properties.Access.Default.AccessToken,
                body,
                cancellationToken
            );
        }

        private async void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            await LikeSong(this.TrackId);
        }
    }
}
