using System.Drawing;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace VeeNet.Services;

public sealed class TrayIconAnimator : IDisposable
{
    private const int IconSize = 16;
    private const int BarCount = 4;
    private const int TickIntervalMs = 120;

    private readonly NotifyIcon _trayIcon;
    private readonly MediaSessionService _mediaService;
    private Timer? _timer;
    private readonly float[] _barHeights = new float[BarCount];
    private readonly Random _rng = new();
    private int _tick;
    private bool _disposed;

    public TrayIconAnimator(NotifyIcon trayIcon, MediaSessionService mediaService)
    {
        _trayIcon = trayIcon;
        _mediaService = mediaService;
    }

    public void Start()
    {
        _trayIcon.Icon = DrawIcon();
        _timer = new Timer(Tick, null, 0, TickIntervalMs);
    }

    private void Tick(object? state)
    {
        bool playing = _mediaService.IsPlaying;
        _tick++;

        if (playing)
        {
            for (int i = 0; i < BarCount; i++)
            {
                float phase = MathF.Sin((_tick * 0.35f) + i * 1.8f) * 0.5f + 0.5f;
                float jitter = 0.15f * (float)_rng.NextDouble();
                float target = MathF.Min(1f, phase + jitter);
                _barHeights[i] = _barHeights[i] * 0.3f + target * 0.7f;
            }
        }
        else
        {
            for (int i = 0; i < BarCount; i++)
                _barHeights[i] = _barHeights[i] * 0.8f;
        }

        try
        {
            var newIcon = DrawIcon();
            var oldIcon = _trayIcon.Icon;
            _trayIcon.Icon = newIcon;
            oldIcon?.Dispose();
        }
        catch { }
    }

    private Icon DrawIcon()
    {
        var bmp = new Bitmap(IconSize, IconSize);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.FromArgb(255, 20, 20, 24));

            int barW = 3;
            int gap = 1;
            int totalW = BarCount * barW + (BarCount - 1) * gap;
            int x0 = (IconSize - totalW) / 2;
            int maxH = IconSize - 2;

            for (int i = 0; i < BarCount; i++)
            {
                float h = _barHeights[i];
                int barH = Math.Clamp((int)(maxH * h), 0, maxH);
                if (barH < 1) continue;

                int x = x0 + i * (barW + gap);
                int y = IconSize - 1 - barH;

                Color c;
                if (h < 0.4f)
                    c = Color.FromArgb(255, 0, 230, 118);
                else if (h < 0.7f)
                    c = Color.FromArgb(255, 255, 214, 10);
                else
                    c = Color.FromArgb(255, 255, 55, 95);

                using var brush = new SolidBrush(c);
                g.FillRectangle(brush, x, y, barW, barH);
            }

            bool allFlat = true;
            for (int i = 0; i < BarCount; i++)
                if (_barHeights[i] > 0.02f) { allFlat = false; break; }

            if (allFlat)
            {
                using var dimBrush = new SolidBrush(Color.FromArgb(255, 80, 80, 85));
                g.FillRectangle(dimBrush, 5, 13, 6, 2);
            }
        }

        IntPtr hIcon = bmp.GetHicon();
        var icon = (Icon)Icon.FromHandle(hIcon).Clone();
        DestroyIcon(hIcon);
        bmp.Dispose();
        return icon;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr handle);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer?.Dispose();
        _timer = null;
    }
}
