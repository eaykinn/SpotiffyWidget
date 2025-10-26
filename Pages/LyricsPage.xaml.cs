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

namespace SpotiffyWidget.Pages
{
    /// <summary>
    /// Interaction logic for LyricsPage.xaml
    /// </summary>
    public partial class LyricsPage : Page
    {
        public LyricsPage(string ArtistName, string SongName, string Lyrics)
        {
            InitializeComponent();
            LoadPage(ArtistName, SongName, Lyrics);
        }

        private void LoadPage(string ArtistName, string SongName, string Lyrics)
        {
            LyricsTextBox.Text = Lyrics;
            ArtistNameLabel.Content = ArtistName;
            SongNameLabel.Content = SongName;
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
    }
}
