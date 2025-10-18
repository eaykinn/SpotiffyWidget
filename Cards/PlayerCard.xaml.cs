using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using HandyControl.Controls;
using SpotiffyWidget.Helpers;
using SpotiffyWidget.Properties;
using SpotiffyWidget.Requests;
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
                        VolumeSlider.Value = playBackState.Device.VolumePercent;
                    });
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
                uiTimer.Stop();
            }
            else
            {
                await PlayerRequests.Play(Access.Default.AccessToken, null, cancellationToken);
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
            _semaphore.Release();
        }
    }

    private void ShuffleButton_Click(object sender, RoutedEventArgs e)
    {
        _ = ShuffleAsync(true);
    }

    private async Task ShuffleAsync(bool isShuffle)
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

            await PlayerRequests.ShufflePlayBack(
                Access.Default.AccessToken,
                isShuffle,
                cancellationToken
            );
        }
        finally
        {
            _semaphore.Release();
        }
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
}
