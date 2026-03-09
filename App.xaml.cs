using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using VeeNet.Services;
using Application = System.Windows.Application;

namespace VeeNet;

public partial class App : Application
{
    private NotifyIcon? _trayIcon;
    private AudioCaptureService? _audioService;
    private TrayIconAnimator? _animator;
    private MediaSessionService? _mediaService;
    private FlyoutWindow? _flyout;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Asterisk,
            Visible = true,
            Text = "VeeNet",
        };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Exit", null, (_, _) =>
        {
            _trayIcon.Visible = false;
            Shutdown();
        });
        _trayIcon.ContextMenuStrip = contextMenu;
        _trayIcon.MouseClick += TrayIcon_MouseClick;

        _audioService = new AudioCaptureService();
        try
        {
            _audioService.Start();
            _animator = new TrayIconAnimator(_trayIcon, _audioService);
            _animator.Start();
        }
        catch
        {
        }

        _mediaService = new MediaSessionService();
        await _mediaService.InitializeAsync();
    }

    private void TrayIcon_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;

        if (_flyout == null)
        {
            _flyout = new FlyoutWindow(_mediaService!);
        }

        if (_flyout.IsVisible)
        {
            _flyout.Hide();
        }
        else
        {
            _flyout.Show();
            _flyout.PositionAboveTray();
            _flyout.Activate();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _animator?.Dispose();
        _audioService?.Dispose();
        _mediaService?.Dispose();

        if (_trayIcon != null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }

        base.OnExit(e);
    }
}