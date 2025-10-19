using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using SpotiffyWidget.Helpers;
using SpotiffyWidget.Properties;
using SpotiffyWidget.Requests;
using static HandyControl.Tools.Interop.InteropValues;
using static SpotiffyWidget.Helpers.CancellationService;

namespace SpotiffyWidget.Cards;

/// <summary>
///     Interaction logic for TrackCard.xaml
/// </summary>
public partial class PlayerCard : UserControl
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly DispatcherTimer uiTimer = new();
    private CancellationTokenSource? _volumeCts;

    public PlayerCard()
    {
        InitializeComponent();
        Loaded += PlayerCard_Loaded;
    }

    private async void PlayerCard_Loaded(object sender, RoutedEventArgs e)
    {
        uiTimer.Interval = TimeSpan.FromMilliseconds(700);
        uiTimer.Tick += UpdateSongInfo;
        uiTimer.Start();

        await Task.Delay(1000);
        await GetPlayBackVolumeAsync();
    }

    private async Task GetPlayBackVolumeAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!await SpotifyAuth.GrantAccess())
                return;

            if (!await SpotifyAuth.CheckDevice())
                return;

            Reset();
            var cancellationToken = Token;

            try
            {
                var playBackState = await PlayerRequests.GetPlayBackState(
                    Access.Default.AccessToken,
                    cancellationToken
                );
                if (playBackState?.Device != null)
                    await Dispatcher.InvokeAsync(() =>
                    {
                        int volume = playBackState.Device.VolumePercent;

                        VolumeIconChange(volume);

                        VolumeSlider.Value = playBackState.Device.VolumePercent;

                        if (playBackState.ShuffleState)
                        {
                            ShuffleButton.IsChecked = true;
                        }
                        else
                        {
                            ShuffleButton.IsChecked = false;
                        }

                        if (playBackState.RepeatState == "off")
                        {
                            RepeatModeButton.IsChecked = false;
                            RepeatModeButton.Tag = Application.Current.FindResource("RepeatIcon");
                        }
                        else if (playBackState.RepeatState == "track")
                        {
                            RepeatModeButton.IsChecked = true;
                            RepeatModeButton.Tag = Application.Current.FindResource(
                                "RepeatTrackIcon"
                            );
                        }
                        else
                        {
                            RepeatModeButton.IsChecked = null;
                            RepeatModeButton.Tag = Application.Current.FindResource("RepeatIcon");
                        }
                    });

                if (playBackState.IsPlaying)
                {
                    var geometry = (Geometry)Application.Current.FindResource("PauseIcon");
                    IconElement.SetGeometry(PlayPauseButton, geometry);
                }
                else
                {
                    var geometry = (Geometry)Application.Current.FindResource("PlayIcon");
                    IconElement.SetGeometry(PlayPauseButton, geometry);
                }
            }
            catch (OperationCanceledException oce)
            {
                if (cancellationToken.IsCancellationRequested)
                    Growl.Info("İşlem kullanıcı tarafından iptal edildi.");

                Growl.Warning(
                    "İstek zaman aşımına uğradı veya dışarıdan bir iptal oldu: " + oce.Message
                );
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void UpdateSongInfo(object? sender, EventArgs e)
    {
        NPSMLibFunctions.UpdatePlayback();
        Cover.Source = NPSMLibFunctions.Image;
        SongName.Content = NPSMLibFunctions.SongName;
        ArtistName.Content = NPSMLibFunctions.ArtistName;
        AlbumName.Content = NPSMLibFunctions.AlbumName;
        MaxTime.Content = NPSMLibFunctions.MaxTime;
        CurrentTime.Content = NPSMLibFunctions.CurrentTime;

        PlayerSlider.Interval = 1;
        PlayerSlider.Maximum = NPSMLibFunctions.MaxSeconds;
        PlayerSlider.Value = NPSMLibFunctions.CurrentSecond;
    }

    private async void PlayPause()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!await SpotifyAuth.GrantAccess())
                return;

            if (!await SpotifyAuth.CheckDevice())
                return;

            Reset();
            var cancellationToken = Token;

            var playBackState = await PlayerRequests.GetPlayBackState(
                Access.Default.AccessToken,
                cancellationToken
            );

            if (playBackState == null)
                return;

            if (playBackState.IsPlaying)
            {
                await PlayerRequests.Pause(Access.Default.AccessToken, null, cancellationToken);
                var geometry = (Geometry)Application.Current.FindResource("PlayIcon");
                IconElement.SetGeometry(PlayPauseButton, geometry);
                uiTimer.Stop();
            }
            else
            {
                await PlayerRequests.Play(Access.Default.AccessToken, null, cancellationToken);
                var geometry = (Geometry)Application.Current.FindResource("PauseIcon");
                IconElement.SetGeometry(PlayPauseButton, geometry);
                uiTimer.Start();
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        PlayPause();
    }

    private async void NextButton_Click(object sender, RoutedEventArgs e)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!await SpotifyAuth.GrantAccess())
                return;

            if (!await SpotifyAuth.CheckDevice())
                return;

            Reset();
            var cancellationToken = Token;
            await PlayerRequests.Next(Access.Default.AccessToken, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!await SpotifyAuth.GrantAccess())
                return;

            if (!await SpotifyAuth.CheckDevice())
                return;

            Reset();
            var cancellationToken = Token;

            await PlayerRequests.Previous(Access.Default.AccessToken, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async void ShowVolumeSlider(object sender, MouseEventArgs e)
    {
        await GetPlayBackVolumeAsync();
        VolumePopup.IsOpen = true;
    }

    //private void HideVolumeSlider(object sender, MouseEventArgs e)
    //{
    //    VolumeSliderBorder.Visibility = Visibility.Hidden;
    //}

    private void VolumeSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        var volume = VolumeSlider.Value;
        _ = SetVolumeAsync(volume);
    }

    private void VolumeIconChange(int volume)
    {
        if (volume <= 0)
        {
            var geometry = (Geometry)Application.Current.FindResource("SoundMuteIcon");
            IconElement.SetGeometry(VolumeButton, geometry);
        }
        else if (volume > 0 && volume <= 33)
        {
            var geometry = (Geometry)Application.Current.FindResource("SoundLowIcon");
            IconElement.SetGeometry(VolumeButton, geometry);
        }
        else if (volume > 33 && volume <= 66)
        {
            var geometry = (Geometry)Application.Current.FindResource("SoundMidIcon");
            IconElement.SetGeometry(VolumeButton, geometry);
        }
        else if (volume > 66)
        {
            var geometry = (Geometry)Application.Current.FindResource("SoundHighIcon");
            IconElement.SetGeometry(VolumeButton, geometry);
        }
    }

    private async Task SetVolumeAsync(double volume)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!await SpotifyAuth.GrantAccess())
                return;
            if (!await SpotifyAuth.CheckDevice())
                return;

            Reset();
            var cancellationToken = Token;

            try
            {
                await PlayerRequests.SetVolume(
                    Access.Default.AccessToken,
                    (int)volume,
                    cancellationToken
                );
            }
            catch (OperationCanceledException oce)
            {
                if (cancellationToken.IsCancellationRequested)
                    Growl.Info("İşlem kullanıcı tarafından iptal edildi.");
                else
                    Growl.Warning(
                        "İstek zaman aşımına uğradı veya dışarıdan bir iptal oldu: " + oce.Message
                    );
            }
            catch (Exception ex)
            {
                Growl.Error(ex.Message);
            }
        }
        finally
        {
            VolumeIconChange((int)volume);
            _semaphore.Release();
        }
    }

    private void ShuffleButton_Click(object sender, RoutedEventArgs e)
    {
        var btn = sender as ToggleButton;
        if (btn == null)
            return;

        // Basıldığında aktif hale getir (Foreground rengi değişir)
        if (btn.IsChecked == true)
        {
            _ = ShuffleAsync(true);
        }
        else
        {
            _ = ShuffleAsync(false);
        }
    }

    private async Task<bool> ShuffleAsync(bool isShuffle)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!await SpotifyAuth.GrantAccess())
                return false;
            if (!await SpotifyAuth.CheckDevice())
                return false;
            Reset();
            var cancellationToken = Token;

            var response = await PlayerRequests.ShufflePlayBack(
                Access.Default.AccessToken,
                isShuffle,
                cancellationToken
            );

            if (!response)
            {
                _semaphore.Release();
                return false;
            }
        }
        finally
        {
            _semaphore.Release();
        }
        return true;
    }

    private async void PlayerSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        uiTimer.Stop();
        var position = (int)PlayerSlider.Value * 1000;
        await SetPositionAync(position);
    }

    private async Task SetPositionAync(int position)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!await SpotifyAuth.GrantAccess())
                return;
            if (!await SpotifyAuth.CheckDevice())
                return;
            Reset();
            var cancellationToken = Token;

            await PlayerRequests.SeekTo(Access.Default.AccessToken, position, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
            uiTimer.Start();
        }
    }

    private void PlayerSlider_DragEnter(object sender, MouseButtonEventArgs e)
    {
        uiTimer.Stop();
    }

    private void PlayerSlider_DragOver(object sender, DragEventArgs e)
    {
        uiTimer.Start();
    }

    private void StackPanel_MouseLeave(object sender, MouseEventArgs e)
    {
        if (!VolumePopup.IsMouseOver && !VolumeButton.IsMouseOver)
            VolumePopup.IsOpen = false;
    }

    private void StackPanel_LostMouseCapture(object sender, MouseEventArgs e)
    {
        if (!VolumePopup.IsMouseOver && !VolumeButton.IsMouseOver)
            VolumePopup.IsOpen = false;
    }

    private async void RepeatModeButton_Click(object sender, RoutedEventArgs e)
    {
        var btn = sender as ToggleButton;
        if (btn == null)
            return;

        // Basıldığında aktif hale getir (Foreground rengi değişir)
        if (btn.IsChecked == true)
        {
            bool isSuccess = await RepeatAsync("track");
            if (isSuccess)
                RepeatModeButton.Tag = Application.Current.FindResource("RepeatTrackIcon");
        }
        else if (btn.IsChecked == null)
        {
            bool isSuccess = await RepeatAsync("context");
            if (isSuccess)
                RepeatModeButton.Tag = Application.Current.FindResource("RepeatIcon");
        }
        else
        {
            bool isSuccess = await RepeatAsync("off");
            if (isSuccess)
                RepeatModeButton.Tag = Application.Current.FindResource("RepeatIcon");
        }
    }

    private async Task<bool> RepeatAsync(string context)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!await SpotifyAuth.GrantAccess())
                return false;
            if (!await SpotifyAuth.CheckDevice())
                return false;
            Reset();
            var cancellationToken = Token;

            var response = await PlayerRequests.RepeatMode(
                Access.Default.AccessToken,
                context,
                cancellationToken
            );

            if (!response)
            {
                _semaphore.Release();
                return false;
            }
        }
        finally
        {
            _semaphore.Release();
        }
        return true;
    }
}
