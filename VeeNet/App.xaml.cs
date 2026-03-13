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

        var prevItem = contextMenu.Items.Add("⏮ Previous", null, async (_, _) => await _mediaService!.PreviousAsync());
        var playPauseItem = contextMenu.Items.Add("⏯ Play/Pause", null, async (_, _) => await _mediaService!.PlayPauseAsync());
        var nextItem = contextMenu.Items.Add("⏭ Next", null, async (_, _) => await _mediaService!.NextAsync());

        contextMenu.Items.Add(new ToolStripSeparator());

        var themeMenu = new ToolStripMenuItem("Bar Theme");
        foreach (var preset in BarTheme.Presets)
        {
            var item = new ToolStripMenuItem(preset.Name);
            item.Click += (_, _) =>
            {
                _animator?.SetTheme(preset);
                foreach (ToolStripItem child in themeMenu.DropDownItems)
                    if (child is ToolStripMenuItem mi) mi.Checked = false;
                item.Checked = true;
            };
            if (preset == BarTheme.Presets[0]) item.Checked = true;
            themeMenu.DropDownItems.Add(item);
        }
        themeMenu.DropDownItems.Add(new ToolStripSeparator());
        var customThemeItem = new ToolStripMenuItem("Custom Theme...");
        customThemeItem.Click += (_, _) =>
        {
            var dialog = new CustomThemeWindow();
            if (dialog.ShowDialog() == true && dialog.ResultTheme != null)
            {
                _animator?.SetTheme(dialog.ResultTheme);
                foreach (ToolStripItem child in themeMenu.DropDownItems)
                    if (child is ToolStripMenuItem mi) mi.Checked = false;
                customThemeItem.Checked = true;
            }
        };
        themeMenu.DropDownItems.Add(customThemeItem);
        contextMenu.Items.Add(themeMenu);

        contextMenu.Items.Add(new ToolStripSeparator());

        var startupItem = new ToolStripMenuItem("Run on Startup")
        {
            CheckOnClick = true,
            Checked = StartupService.IsEnabled,
        };
        startupItem.CheckedChanged += (_, _) => StartupService.SetEnabled(startupItem.Checked);
        contextMenu.Items.Add(startupItem);

        contextMenu.Items.Add(new ToolStripSeparator());

        contextMenu.Items.Add("Exit", null, (_, _) =>
        {
            _trayIcon.Visible = false;
            Shutdown();
        });

        contextMenu.Opening += (_, _) =>
        {
            playPauseItem.Text = _mediaService?.IsPlaying == true ? "⏸ Pause" : "▶ Play";
        };

        _trayIcon.ContextMenuStrip = contextMenu;
        _trayIcon.MouseClick += TrayIcon_MouseClick;

        _mediaService = new MediaSessionService();
        await _mediaService.InitializeAsync();

        _animator = new TrayIconAnimator(_trayIcon, _mediaService);
        _animator.Start();
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
        _mediaService?.Dispose();

        if (_trayIcon != null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }

        base.OnExit(e);
    }
}