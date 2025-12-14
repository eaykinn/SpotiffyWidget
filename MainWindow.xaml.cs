using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Themes;
using HandyControl.Tools;
using SpotiffyWidget.Helpers;
using SpotiffyWidget.Pages;
using SpotiffyWidget.Properties;
using SpotiffyWidget.Requests;

namespace SpotiffyWidget
{
    public partial class MainWindow : System.Windows.Window
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

            // Window handle'ının hazır olması için küçük bir delay
            await Task.Delay(100);

            // Mica effect'i sabit olarak uygula (backdrop type 1 = Mica)
            int isDarkTheme = Properties.UserSettings.Default.Theme == "Dark" ? 1 : 0;
            BlurHelper.EnableMicaEffect(this, isDarkTheme, 1);

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

            if (Properties.UserSettings.Default.CloseOnShutDown)
            {
                StopMusicCB.IsChecked = true;
                SystemEventHelper.StartListening();
                SystemEventHelper.OnSystemEvent += SystemEventHelper_OnSystemEvent;
            }

            if (Properties.UserSettings.Default.PreventSleepMode)
            {
                PowerModeCB.IsChecked = true;
                PowerHelper.PreventSleep();
            }
        }

        #region Change Theme

        private async void ButtonSkins_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button)
            {
                PopupConfig.IsOpen = false; // Close popup after a selection
                if (button.Tag is ApplicationTheme tag)
                {
                    ((App)Application.Current).UpdateTheme(tag);
                    Properties.UserSettings.Default.Theme = tag.ToString();
                    Properties.UserSettings.Default.Save();

                    // Küçük delay ekleyerek UI'ın güncellenmesini bekle
                    await Task.Delay(50);

                    // Tema değişince Mica effect'i yeniden uygula (sabit Mica backdrop type 1)
                    int isDarkTheme = Properties.UserSettings.Default.Theme == "Dark" ? 1 : 0;
                    BlurHelper.EnableMicaEffect(this, isDarkTheme, 1);
                }
                else if (button.Tag is Brush accentTag)
                {
                    ((App)Application.Current).UpdateAccent(accentTag);

                    SolidColorBrush solidBrush = accentTag as SolidColorBrush;
                    Properties.UserSettings.Default.AccentColor =
                        ColorConverterHelper.ToDrawingColor(solidBrush);
                    Properties.UserSettings.Default.Save();
                    
                    // Küçük delay ekleyerek UI'ın güncellenmesini bekle
                    await Task.Delay(50);
                    
                    // Accent değişince de Mica effect'i yeniden uygula (sabit Mica backdrop type 1)
                    int isDarkTheme = Properties.UserSettings.Default.Theme == "Dark" ? 1 : 0;
                    BlurHelper.EnableMicaEffect(this, isDarkTheme, 1);
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

                    picker.Confirmed += async delegate
                    {
                        ((App)Application.Current).UpdateAccent(picker.SelectedBrush);

                        var x = ColorConverterHelper.ToDrawingColor(picker.SelectedBrush);
                        Properties.UserSettings.Default.AccentColor = x;

                        Properties.UserSettings.Default.Save();
                        
                        // Küçük delay ekleyerek UI'ın güncellenmesini bekle
                        await Task.Delay(50);
                        
                        // Accent değişince Mica effect'i yeniden uygula (sabit Mica backdrop type 1)
                        int isDarkTheme = Properties.UserSettings.Default.Theme == "Dark" ? 1 : 0;
                        BlurHelper.EnableMicaEffect(this, isDarkTheme, 1);
                        
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

        private void Button_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

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
            PlayerRow.MinHeight = 0;
            TabRow.MinHeight = 0;
            PlayerBorder.Visibility = Visibility.Collapsed;
            MiniPlayerBorder.Visibility = Visibility.Visible;
            TabsBorder.Visibility = Visibility.Collapsed;
            TitleBarBorder.Visibility = Visibility.Collapsed; // Title bar'ı gizle
            this.Topmost = true;

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

        private void StopMusicCB_Click(object sender, RoutedEventArgs e)
        {
            if (StopMusicCB.IsChecked == true)
            {
                SystemEventHelper.StartListening();
                SystemEventHelper.OnSystemEvent += SystemEventHelper_OnSystemEvent;
                Properties.UserSettings.Default.CloseOnShutDown = true;
            }
            else
            {
                SystemEventHelper.OnSystemEvent -= SystemEventHelper_OnSystemEvent;
                SystemEventHelper.StopListening();
                Properties.UserSettings.Default.CloseOnShutDown = false;
            }
            Properties.UserSettings.Default.Save();
        }

        private async void SystemEventHelper_OnSystemEvent(string state)
        {
            if (state == "SessionLock" || state == "Sleep")
            {
                await PlayerRequests.Pause(Access.Default.AccessToken, null, CancellationToken.None);
            }
        }

        private void PreventSleepMode_Click(object sender, RoutedEventArgs e)
        {
            if (PowerModeCB.IsChecked == true)
            {
                PowerHelper.PreventSleep();
                Growl.Info("Sleep Mode Disabled");
                Properties.UserSettings.Default.PreventSleepMode = true;
            }
            else
            {
                PowerHelper.AllowSleep();
                Growl.Info("Sleep Mode Enabled");
                Properties.UserSettings.Default.PreventSleepMode = false;
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
