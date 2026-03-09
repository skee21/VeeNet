using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Windows.Media.Control;
using Windows.Storage.Streams;

namespace VeeNet.Services;

public sealed class MediaSessionService : INotifyPropertyChanged, IDisposable
{
    private GlobalSystemMediaTransportControlsSessionManager? _manager;
    private GlobalSystemMediaTransportControlsSession? _currentSession;

    private string _title = string.Empty;
    private string _artist = string.Empty;
    private BitmapImage? _albumArt;
    private bool _isPlaying;
    private bool _disposed;

    public string Title
    {
        get => _title;
        private set => SetField(ref _title, value);
    }

    public string Artist
    {
        get => _artist;
        private set => SetField(ref _artist, value);
    }

    public BitmapImage? AlbumArt
    {
        get => _albumArt;
        private set => SetField(ref _albumArt, value);
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        private set => SetField(ref _isPlaying, value);
    }

    public bool HasSession => _currentSession != null;

    public async Task InitializeAsync()
    {
        _manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        _manager.CurrentSessionChanged += OnCurrentSessionChanged;
        AttachSession(_manager.GetCurrentSession());
    }

    private void OnCurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender,
        CurrentSessionChangedEventArgs args)
    {
        AttachSession(sender.GetCurrentSession());
    }

    private void AttachSession(GlobalSystemMediaTransportControlsSession? session)
    {
        if (_currentSession != null)
        {
            _currentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
            _currentSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
        }

        _currentSession = session;

        if (_currentSession != null)
        {
            _currentSession.MediaPropertiesChanged += OnMediaPropertiesChanged;
            _currentSession.PlaybackInfoChanged += OnPlaybackInfoChanged;
            _ = RefreshMetadataAsync();
            RefreshPlaybackInfo();
        }
        else
        {
            Title = string.Empty;
            Artist = string.Empty;
            AlbumArt = null;
            IsPlaying = false;
        }

        OnPropertyChanged(nameof(HasSession));
    }

    private void OnMediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
    {
        _ = RefreshMetadataAsync();
    }

    private void OnPlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
    {
        RefreshPlaybackInfo();
    }

    private async Task RefreshMetadataAsync()
    {
        if (_currentSession == null) return;

        try
        {
            var props = await _currentSession.TryGetMediaPropertiesAsync();
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                Title = props.Title ?? string.Empty;
                Artist = props.Artist ?? string.Empty;
            });

            if (props.Thumbnail != null)
            {
                var stream = await props.Thumbnail.OpenReadAsync();
                var memStream = new MemoryStream();
                var inputStream = stream.AsStreamForRead();
                await inputStream.CopyToAsync(memStream);
                memStream.Position = 0;

                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = memStream;
                    bmp.EndInit();
                    bmp.Freeze();
                    AlbumArt = bmp;
                });
            }
            else
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() => AlbumArt = null);
            }
        }
        catch
        {
        }
    }

    private void RefreshPlaybackInfo()
    {
        if (_currentSession == null) return;

        try
        {
            var info = _currentSession.GetPlaybackInfo();
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                IsPlaying = info.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
            });
        }
        catch
        {
        }
    }

    public async Task PlayPauseAsync()
    {
        if (_currentSession == null) return;
        try { await _currentSession.TryTogglePlayPauseAsync(); } catch { }
    }

    public async Task NextAsync()
    {
        if (_currentSession == null) return;
        try { await _currentSession.TrySkipNextAsync(); } catch { }
    }

    public async Task PreviousAsync()
    {
        if (_currentSession == null) return;
        try { await _currentSession.TrySkipPreviousAsync(); } catch { }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_currentSession != null)
        {
            _currentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
            _currentSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
        }

        if (_manager != null)
        {
            _manager.CurrentSessionChanged -= OnCurrentSessionChanged;
        }
    }
}
