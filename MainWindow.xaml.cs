using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Themes;
using HandyControl.Tools;
using SpotiffyWidget.Helpers;
using SpotiffyWidget.Pages;

namespace SpotiffyWidget
{
    public partial class MainWindow
    {
        private bool _isMiniSize = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                TabsFrame.Navigate(new TabsPage());
                var app = (App)Application.Current;
                //app.SetCustomBlurValue(); // Uygulama başlatıldığında bulanıklık
                await WindowLoaded();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        private async Task WindowLoaded()
        {
            var accentColor = ColorConverterHelper.ToSolidColorBrush(
                Properties.UserSettings.Default.AccentColor
            );
            ((App)Application.Current).UpdateAccent(accentColor);

            if (Properties.UserSettings.Default.Theme == "Dark")
            {
                ((App)Application.Current).UpdateTheme(ApplicationTheme.Dark);
            }
            else
            {
                ((App)Application.Current).UpdateTheme(ApplicationTheme.Light);
            }

            if (!Properties.UserSettings.Default.AppSize)
                await ChangeToMiniSize();

            if (Properties.UserSettings.Default.OpenSpotifyAtStart)
            {
                OpenSpotifyCB.IsChecked = true;
                Process.Start("spotify.exe");
            }

            if (Properties.UserSettings.Default.AllwaysOnTop)
            {
                AlwaysOnTopCB.IsChecked = true;
                this.Topmost = true;
            }

            if (Properties.UserSettings.Default.PlaceOnDesktop)
            {
                PlaceOnDesktopCB.IsChecked = true;
                DesktopHelper.SetAsDesktopChild(this);
            }
        }

        #region Change Theme

        private void ButtonSkins_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button)
            {
                PopupConfig.IsOpen = false; // Close popup after a selection
                if (button.Tag is ApplicationTheme tag)
                {
                    ((App)Application.Current).UpdateTheme(tag);
                    Properties.UserSettings.Default.Theme = tag.ToString();
                    Properties.UserSettings.Default.Save();
                }
                else if (button.Tag is Brush accentTag)
                {
                    ((App)Application.Current).UpdateAccent(accentTag);

                    SolidColorBrush solidBrush = accentTag as SolidColorBrush;
                    Properties.UserSettings.Default.AccentColor =
                        ColorConverterHelper.ToDrawingColor(solidBrush);
                    Properties.UserSettings.Default.Save();
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

                    picker.Confirmed += delegate
                    {
                        ((App)Application.Current).UpdateAccent(picker.SelectedBrush);
                        Properties.UserSettings.Default.AccentColor =
                            ColorConverterHelper.ToDrawingColor(picker.SelectedBrush);
                        Properties.UserSettings.Default.Save();
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

        private void ChangeTheme(object sender, RoutedEventArgs e)
        {
            PopupConfig.IsOpen = !PopupConfig.IsOpen;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) { }

        private void MenuItemOpen(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Uygulamayı kapatma işlemini iptal et
            e.Cancel = true;

            // Pencereyi gizle
            this.Hide();

            Growl.InfoGlobal(
                new GrowlInfo
                {
                    Message = "Spotify Widget running background...",
                    ShowDateTime = true,
                    StaysOpen = false,
                    WaitTime = 3,
                    Token = "MainWindow",
                }
            );

            base.OnClosing(e);
        }

        private async void ChangeSizeClick(object sender, RoutedEventArgs e)
        {
            await ChangeToMiniSize();
        }

        private async Task ChangeToMiniSize()
        {
            //this.Height = 120;
            //this.Width = 360;
            //this.MaxHeight = 120;
            //this.MaxWidth = 360;
            //this.MinHeight = 120;
            //this.MinWidth = 360;
            //MiniPlayerBorder.Height = 110;
            //MiniPlayerBorder.Width = 350;
            //PlayerRow.MinHeight = 0;
            //TabRow.MinHeight = 0;
            //PlayerBorder.Visibility = Visibility.Collapsed;
            //MiniPlayerBorder.Visibility = Visibility.Visible;
            //MiniPlayerBorder.Padding = new Thickness(0.0, 0.0, 0.0, 0.0);
            //TabsBorder.Visibility = Visibility.Collapsed;
            //this.Topmost = true;

            //this.ShowNonClientArea = false;

            //this.UpdateLayout();
            //this.InvalidateVisual();

            var currentState = this.WindowState;
            this.WindowState = WindowState.Minimized;

            this.Height = 120;
            this.Width = 360;
            this.MaxHeight = 120;
            this.MaxWidth = 360;
            this.MinHeight = 120;
            this.MinWidth = 360;
            MiniPlayerBorder.Height = 110;
            MiniPlayerBorder.Width = 350;
            PlayerRow.MinHeight = 0;
            TabRow.MinHeight = 0;
            PlayerBorder.Visibility = Visibility.Collapsed;
            MiniPlayerBorder.Visibility = Visibility.Visible;
            MiniPlayerBorder.Padding = new Thickness(0.0);
            TabsBorder.Visibility = Visibility.Collapsed;
            this.Topmost = true;
            this.ShowNonClientArea = false;

            await Task.Delay(100);
            this.WindowState = currentState;

            _isMiniSize = true;
            Properties.UserSettings.Default.AppSize = false;
            Properties.UserSettings.Default.Save();
        }

        private void PlaceOnDesktopCB_Click(object sender, RoutedEventArgs e)
        {
            if (PlaceOnDesktopCB.IsChecked == true)
            {
                DesktopHelper.SetAsDesktopChild(this);
                Properties.UserSettings.Default.PlaceOnDesktop = true;
            }
            else
            {
                Properties.UserSettings.Default.PlaceOnDesktop = false;
            }
            Properties.UserSettings.Default.Save();
        }

        private void OpenSpotifyCB_Click(object sender, RoutedEventArgs e)
        {
            if (OpenSpotifyCB.IsChecked == true)
            {
                Properties.UserSettings.Default.OpenSpotifyAtStart = true;
            }
            else
            {
                Properties.UserSettings.Default.OpenSpotifyAtStart = false;
            }

            Properties.UserSettings.Default.Save();
        }

        private void AlwaysOnTopCB_Click(object sender, RoutedEventArgs e)
        {
            if (AlwaysOnTopCB.IsChecked == true)
            {
                this.Topmost = true;
                Properties.UserSettings.Default.AllwaysOnTop = true;
            }
            else
            {
                this.Topmost = false;
                Properties.UserSettings.Default.AllwaysOnTop = true;
            }
            Properties.UserSettings.Default.Save();
        }
    }
}
