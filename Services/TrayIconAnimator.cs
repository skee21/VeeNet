using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace VeeNet.Services;

public sealed class TrayIconAnimator : IDisposable
{
    private const int IconSize = 16;
    private const int BarCount = 5;
    private const int FrameCount = 6;
    private const int TickIntervalMs = 80;

    private readonly NotifyIcon _trayIcon;
    private readonly AudioCaptureService _audioService;
    private readonly Icon[] _frames;
    private Timer? _timer;
    private int _lastFrameIndex = -1;
    private bool _disposed;

    public TrayIconAnimator(NotifyIcon trayIcon, AudioCaptureService audioService)
    {
        _trayIcon = trayIcon;
        _audioService = audioService;
        _frames = GenerateFrames();
    }

    public void Start()
    {
        _trayIcon.Icon = _frames[0];
        _timer = new Timer(Tick, null, 0, TickIntervalMs);
    }

    private void Tick(object? state)
    {
        float peak = _audioService.CurrentPeak;

        int frameIndex = peak switch
        {
            < 0.02f => 0,
            < 0.10f => 1,
            < 0.25f => 2,
            < 0.45f => 3,
            < 0.70f => 4,
            _       => 5,
        };

        if (frameIndex == _lastFrameIndex)
            return;

        _lastFrameIndex = frameIndex;

        try
        {
            _trayIcon.Icon = _frames[frameIndex];
        }
        catch
        {
        }
    }

    private static Icon[] GenerateFrames()
    {
        var frames = new Icon[FrameCount];

        for (int f = 0; f < FrameCount; f++)
        {
            using var bmp = new Bitmap(IconSize, IconSize);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);

            if (f == 0)
            {
                using var pen = new Pen(Color.FromArgb(180, 100, 100, 100), 1);
                g.DrawLine(pen, 1, IconSize - 3, IconSize - 2, IconSize - 3);
            }
            else
            {
                float level = f / (float)(FrameCount - 1);
                int barWidth = (IconSize - 2) / BarCount;
                int gap = 1;

                for (int b = 0; b < BarCount; b++)
                {
                    float barLevel = Math.Min(1.0f, level * (0.6f + 0.4f * ((b + f) % BarCount) / (float)(BarCount - 1)));
                    int barHeight = Math.Max(2, (int)((IconSize - 4) * barLevel));
                    int x = 1 + b * (barWidth + gap);
                    int y = IconSize - 2 - barHeight;
                    Color barColor = barLevel switch
                    {
                        < 0.4f => Color.FromArgb(220, 76, 175, 80),
                        < 0.7f => Color.FromArgb(220, 255, 193, 7),
                        _      => Color.FromArgb(220, 244, 67, 54),
                    };

                    using var brush = new SolidBrush(barColor);
                    g.FillRectangle(brush, x, y, barWidth, barHeight);
                }
            }

            IntPtr hIcon = bmp.GetHicon();
            frames[f] = Icon.FromHandle(hIcon).Clone() as Icon ?? SystemIcons.Application;
            DestroyIcon(hIcon);
        }

        return frames;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr handle);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _timer?.Dispose();
        _timer = null;

        foreach (var frame in _frames)
            frame?.Dispose();
    }
}
