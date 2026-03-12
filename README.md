# VeeNet

A lightweight Windows system tray utility that provides media playback controls and real-time audio visualization.

## Features

- **System Tray Integration** — Runs quietly in the system tray with a dynamic animated icon.
- **Audio Visualizer** — Real-time animated equalizer bars on the tray icon that respond to playback state, with color-coded levels (green → yellow → red).
- **Media Controls** — Play/Pause, Next, and Previous track controls via right-click context menu.
- **Flyout Media Controller** — A sleek, borderless popup window on left-click showing album art, track title, artist, and transport controls.
- **Run on Startup** — Optional auto-start with Windows via a toggleable context menu option.

## Tech Stack

- **WPF** (.NET 8, Windows 10+)
- **Windows.Media.Control** — System media session integration (SMTC)
- **NAudio** — Audio capture and processing
- **System.Drawing / GDI+** — Dynamic tray icon rendering

## Project Structure

```
VeeNet/
├── App.xaml / App.xaml.cs          # Application entry point and tray icon setup
├── FlyoutWindow.xaml / .cs         # Media controller flyout popup
├── Services/
│   ├── MediaSessionService.cs      # Windows media session manager integration
│   ├── TrayIconAnimator.cs         # Animated equalizer tray icon renderer
│   └── StartupService.cs          # Registry-based startup management
├── VeeNet.csproj                   # Project configuration
└── AssemblyInfo.cs                 # Assembly metadata
```

## Building

```bash
dotnet build
```

## Running

```bash
dotnet run
```

## License

This project is unlicensed. All rights reserved.
