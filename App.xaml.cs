using System.Windows;
using System.Windows.Media;
using HandyControl.Themes;

namespace SpotiffyWidget
{
    public partial class App : Application
    {
        //internal void UpdateTheme(ApplicationTheme theme)
        //{
        //    if (ThemeManager.Current.ApplicationTheme != theme)
        //    {
        //        ThemeManager.Current.ApplicationTheme = theme;
        //    }
        //}

        //internal void UpdateAccent(Brush accent)
        //{
        //    if (ThemeManager.Current.AccentColor != accent)
        //    {
        //        ThemeManager.Current.AccentColor = accent;
        //    }
        //}

        public void SetCustomBlurValue()
        {
            // DOĞRU KOD:
            Application.Current.Resources["BlurGradientValue"] = 0x33333333u;
        }

        // App constructor'ı (veya OnStartup) içine eklemiştin
        public App()
        {
            // Constructor'ın olmadığı varsayılan bir proje için OnStartup daha garantidir
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetCustomBlurValue(); // İlk açılışta ayarla
        }

        // Bu metotlar sende zaten vardı
        internal void UpdateTheme(ApplicationTheme theme)
        {
            ThemeManager.Current.ApplicationTheme = theme;
            SetCustomBlurValue(); // Tema değişince TEKRAR ayarla
        }

        internal void UpdateAccent(Brush brush)
        {
            ThemeManager.Current.AccentColor = brush;
            SetCustomBlurValue(); // Renk değişince TEKRAR ayarla
        }
    }
}
