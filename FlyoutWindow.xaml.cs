using System.ComponentModel;
using System.Windows;
using VeeNet.Services;

namespace VeeNet;

public partial class FlyoutWindow : Window
{
    private readonly MediaSessionService _mediaService;

    public FlyoutWindow(MediaSessionService mediaService)
    {
        InitializeComponent();
        _mediaService = mediaService;
        _mediaService.PropertyChanged += MediaService_PropertyChanged;

        RefreshUI();
    }

    private void MediaService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Dispatcher.Invoke(RefreshUI);
    }

    private void RefreshUI()
    {
        TitleText.Text = string.IsNullOrEmpty(_mediaService.Title) ? "Not Playing" : _mediaService.Title;
        ArtistText.Text = _mediaService.Artist ?? string.Empty;
        AlbumArtImage.Source = _mediaService.AlbumArt;

        PlayPauseBtn.Content = _mediaService.IsPlaying ? "\uE769" : "\uE768";
    }

    public void PositionAboveTray()
    {
        var workArea = SystemParameters.WorkArea;

        Left = workArea.Right - ActualWidth - 12;
        Top = workArea.Bottom - ActualHeight - 8;

        if (double.IsNaN(ActualWidth) || ActualWidth == 0)
        {
            Left = workArea.Right - 360;
            Top = workArea.Bottom - 200;
        }
    }

    private void Window_Deactivated(object? sender, EventArgs e)
    {
        Hide();
    }

    private async void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        await _mediaService.PlayPauseAsync();
    }

    private async void NextButton_Click(object sender, RoutedEventArgs e)
    {
        await _mediaService.NextAsync();
    }

    private async void PrevButton_Click(object sender, RoutedEventArgs e)
    {
        await _mediaService.PreviousAsync();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}
