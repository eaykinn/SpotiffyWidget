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
using SpotiffyWidget.Models;
using SpotiffyWidget.Pages;
using SpotiffyWidget.SpotifyEndPoints;

namespace SpotiffyWidget.Cards
{
    /// <summary>
    /// Interaction logic for TrackCard.xaml
    /// </summary>
    public partial class MyPlayListsCard : UserControl
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string ImageUri { get; set; }

        public MyPlayListsCard()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Type == "Album")
            {
                var mw = Application.Current.MainWindow as MainWindow;
                if (mw != null)
                {
                    mw.TabsFrame.Navigate(new TracksPage(Id, ImageUri, 0));
                    ;
                }
            }

            if (Type == "PlayList")
            {
                var mw = Application.Current.MainWindow as MainWindow;
                if (mw != null)
                {
                    mw.TabsFrame.Navigate(new TracksPage(Id, ImageUri, 1));
                    ;
                }
            }
        }
    }
}
