using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
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
using Newtonsoft.Json.Linq;
using SpotiffyWidget.Cards;
using SpotiffyWidget.Helpers;
using SpotiffyWidget.Models;
using SpotiffyWidget.Requests;

namespace SpotiffyWidget.Pages
{
    /// <summary>
    /// Interaction logic for ArtistDetailPage.xaml
    /// </summary>
    public partial class ArtistDetailPage : Page
    {
        private string _artistId;
        private int _currentOffset = 0;
        private int _MaxOffset = 0;
        public ObservableCollection<ArtistInfo> Artists { get; set; }
        public ObservableCollection<AlbumInfo> Albums { get; set; }

        public class ArtistInfo
        {
            public string Popularity { get; set; }
            public string Followers { get; set; }
            public string Genres { get; set; }
        }

        public class AlbumInfo
        {
            public string PlayListName { get; set; }
            public string Owner { get; set; }
            public string ReleaseDate { get; set; }
            public int NumberOfTracks { get; set; }
            public string CoverUrl { get; set; }
            public string Id { get; set; }
            public string Type { get; set; }
            public string ImageUri { get; set; }
        }

        public ArtistDetailPage(string ArtistId)
        {
            InitializeComponent();
            _artistId = ArtistId;
            LoadArtistInfo(_artistId);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var mw = Application.Current.MainWindow as MainWindow;
            if (mw != null)
            {
                if (mw.TabsFrame.CanGoBack)
                    mw.TabsFrame.GoBack();
            }
        }

        private async void LoadArtistInfo(string artistId)
        {
            CancellationService.Reset();
            var cancellationToken = new CancellationToken();

            var artist = await ArtistsRequests.GetArtist(
                Properties.Access.Default.AccessToken,
                artistId,
                cancellationToken
            );

            int totalFollowers = 0;
            if (artist.Followers != null)
            {
                var obj = JObject.Parse(artist.Followers.ToString());
                totalFollowers = (int)obj["total"];
            }

            Artists = new ObservableCollection<ArtistInfo>
            {
                new ArtistInfo
                {
                    Popularity = artist.Popularity.ToString(),
                    Followers = totalFollowers.ToString(),
                    Genres = artist.Genres.FirstOrDefault(),
                },
            };

            DataGridArtistInfo.ItemsSource = Artists;

            ArtistName.Text = artist.Name;
            ArtistImage.Source = new BitmapImage(new Uri(artist.Images.FirstOrDefault().Url));

            var albums = await ArtistsRequests.GetArtistAlbums(
                Properties.Access.Default.AccessToken,
                artistId,
                cancellationToken
            );

            _MaxOffset = albums.Count - 1;

            Albums = new ObservableCollection<AlbumInfo>();
            foreach (var album in albums)
            {
                Albums.Add(
                    new AlbumInfo
                    {
                        NumberOfTracks = album.TotalTracks,
                        Owner = album.Name,
                        PlayListName = album.Name,
                        CoverUrl = album.Image.FirstOrDefault().Url,
                        ReleaseDate = album.ReleaseDate,
                        Id = album.Id,
                        ImageUri = album.Image.FirstOrDefault().Url,
                    }
                );
            }

            MyPlayListsCard card = new MyPlayListsCard();
            card.PlayListName.Content = Albums[_currentOffset].PlayListName;
            card.NumberOfTracks.Content = Albums[_currentOffset].NumberOfTracks;
            card.Owner.Content = Albums[_currentOffset].ReleaseDate;
            card.Cover.Source = new BitmapImage(new Uri(Albums[_currentOffset].CoverUrl));
            card.Id = Albums[_currentOffset].Id;
            card.Type = "Album";
            card.ImageUri = Albums[_currentOffset].CoverUrl;

            AlbumCard.Child = card;
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (_currentOffset > 0)
            {
                _currentOffset--;

                MyPlayListsCard card = new MyPlayListsCard();
                card.PlayListName.Content = Albums[_currentOffset].PlayListName;
                card.NumberOfTracks.Content = Albums[_currentOffset].NumberOfTracks;
                card.Owner.Content = Albums[_currentOffset].ReleaseDate;
                card.Cover.Source = new BitmapImage(new Uri(Albums[_currentOffset].CoverUrl));
                card.Id = Albums[_currentOffset].Id;
                card.Type = "Album";
                card.ImageUri = Albums[_currentOffset].CoverUrl;

                AlbumCard.Child = card;
            }
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (_currentOffset < _MaxOffset)
            {
                _currentOffset++;

                MyPlayListsCard card = new MyPlayListsCard();
                card.PlayListName.Content = Albums[_currentOffset].PlayListName;
                card.NumberOfTracks.Content = Albums[_currentOffset].NumberOfTracks;
                card.Owner.Content = Albums[_currentOffset].ReleaseDate;
                card.Cover.Source = new BitmapImage(new Uri(Albums[_currentOffset].CoverUrl));
                card.Id = Albums[_currentOffset].Id;
                card.Type = "Album";
                card.ImageUri = Albums[_currentOffset].CoverUrl;
                AlbumCard.Child = card;
            }
        }
    }
}
