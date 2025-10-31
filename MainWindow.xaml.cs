using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
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

                await WindowLoaded();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        private async Task WindowLoaded()
        {
            if (Properties.UserSettings.Default.AccentColor.IsEmpty)
            {
                Properties.UserSettings.Default.AccentColor = System.Drawing.Color.BlueViolet;
            }

            bool trasp = Properties.UserSettings.Default.Transparency;
            int mica = Properties.UserSettings.Default.MicaEffect;

            var accentColor = ColorConverterHelper.ToSolidColorBrush(
                Properties.UserSettings.Default.AccentColor
            );
            ((App)Application.Current).UpdateAccent(accentColor);

            if (Properties.UserSettings.Default.Theme == "Dark")
            {
                ((App)Application.Current).UpdateTheme(ApplicationTheme.Dark);

                BlurHelper.EnableMicaEffect(this, 1, mica);
            }
            else
            {
                ((App)Application.Current).UpdateTheme(ApplicationTheme.Light);
                BlurHelper.EnableMicaEffect(this, 0, mica);
            }

            if (trasp)
            {
                this.Background = Brushes.Transparent;
                AllowTransparency.IsChecked = true;
            }
            else
            {
                this.ClearValue(BackgroundProperty);
                AllowTransparency.IsChecked = false;
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
            else
            {
                AlwaysOnTopCB.IsChecked = false;
                this.Topmost = false;
            }

            if (Properties.UserSettings.Default.PlaceOnDesktop)
            {
                PlaceOnDesktopCB.IsChecked = true;
                DesktopHelper.SetAsDesktopChild(this);
            }

            switch (mica)
            {
                case 1:
                {
                    Mica.IsChecked = true;
                    break;
                }
                case 2:
                {
                    MicaAlt.IsChecked = true;
                    break;
                }
                case 3:
                {
                    Acrylic.IsChecked = true;
                    break;
                }
            }
        }

        #region Change Theme

        private void ButtonSkins_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button)
            {
                int mica = Properties.UserSettings.Default.MicaEffect;

                PopupConfig.IsOpen = false; // Close popup after a selection
                if (button.Tag is ApplicationTheme tag)
                {
                    ((App)Application.Current).UpdateTheme(tag);
                    Properties.UserSettings.Default.Theme = tag.ToString();
                    Properties.UserSettings.Default.Save();

                    if (Properties.UserSettings.Default.Theme == "Dark")
                    {
                        BlurHelper.EnableMicaEffect(this, 1, mica);
                    }
                    else
                    {
                        BlurHelper.EnableMicaEffect(this, 0, mica);
                    }
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

                        var x = ColorConverterHelper.ToDrawingColor(picker.SelectedBrush);
                        Properties.UserSettings.Default.AccentColor = x;

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
            var currentState = this.WindowState;
            this.WindowState = WindowState.Minimized;

            this.Height = 130;
            this.Width = 360;
            this.MaxHeight = 130;
            this.MaxWidth = 360;
            this.MinHeight = 130;
            this.MinWidth = 360;
            MiniPlayerBorder.Height = 120;
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
                Properties.UserSettings.Default.AllwaysOnTop = false;
            }
            Properties.UserSettings.Default.Save();
        }

        private void AllowTransparency_Click(object sender, RoutedEventArgs e)
        {
            if (AllowTransparency.IsChecked == true)
            {
                this.Background = Brushes.Transparent;
                Properties.UserSettings.Default.Transparency = true;
            }
            else
            {
                this.ClearValue(BackgroundProperty);
                Properties.UserSettings.Default.Transparency = false;
            }
            Properties.UserSettings.Default.Save();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            int isDarkTheme;
            if (Properties.UserSettings.Default.Theme == "Dark")
            {
                isDarkTheme = 1;
            }
            else
            {
                isDarkTheme = 0;
            }

            var radioButton = sender as RadioButton;

            if (radioButton.Content.ToString() == "Mica")
            {
                BlurHelper.EnableMicaEffect(this, isDarkTheme, 1);
                Properties.UserSettings.Default.MicaEffect = 1;
            }
            else if (radioButton.Content.ToString() == "Mica Alt")
            {
                BlurHelper.EnableMicaEffect(this, isDarkTheme, 2);
                Properties.UserSettings.Default.MicaEffect = 2;
            }
            else if (radioButton.Content.ToString() == "Acrylic")
            {
                BlurHelper.EnableMicaEffect(this, isDarkTheme, 3);
                Properties.UserSettings.Default.MicaEffect = 3;
            }

            Properties.UserSettings.Default.Save();
        }

        private async void TabsFrame_Navigated(
            object sender,
            System.Windows.Navigation.NavigationEventArgs e
        )
        {
            if (TabsFrame.Content is Page page)
            {
                // Görsel dönüşüm ve opaklık sıfırla
                var transform = new TranslateTransform { X = 80 }; // sağdan gelsin
                page.RenderTransform = transform;
                page.Opacity = 0;

                // Bir tick bekle (UI render bitsin)
                await Task.Delay(100);

                // Fade + slide animasyonları
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
                };

                var slideIn = new DoubleAnimation(80, 0, TimeSpan.FromMilliseconds(300))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
                };

                // Uygula
                transform.BeginAnimation(TranslateTransform.XProperty, slideIn);
                page.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            }
        }
    }
}
