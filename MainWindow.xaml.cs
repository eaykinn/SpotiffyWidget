using System;
using System.Windows;
using System.Windows.Controls;
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
        public MainWindow()
        {
            InitializeComponent();
            TabsFrame.Navigate(new TabsPage());
            var app = (App)Application.Current;
            app.SetCustomBlurValue(); // Uygulama başlatıldığında bulanıklık
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

                    picker.Confirmed += delegate
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

            // Baloncuk bildirimi göster (isteğe bağlı)
            NotifyIcon.ShowBalloonTip(
                "Spotify Widget",
                "App running background...",
                NotifyIconInfoType.Info,
                "TrayIcon"
            );

            base.OnClosing(e);
        }
    }
}
