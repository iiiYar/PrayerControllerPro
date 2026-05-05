# Prayer Controller Pro

<p align="center">
  <img src="src/PrayerControllerPro.App/Assets/brand-mark.png" alt="Prayer Controller Pro" width="120" />
</p>

<p align="center">
  Prayer Controller Pro is a Windows desktop application built with .NET 8 and WPF.<br>
  It automates prayer-time-based media control, Adhan and Iqama audio playback,<br>
  reminders, notifications, and system tray behavior.
</p>

<p align="center">
  <a href="https://github.com/iiiYar/PrayerControllerPro/releases">
    <img src="https://img.shields.io/github/v/release/iiiYar/PrayerControllerPro" alt="Latest Release" />
  </a>
  <img src="https://img.shields.io/badge/platform-Windows-blue" alt="Windows" />
  <img src="https://img.shields.io/badge/.NET-8-purple" alt=".NET 8" />
  <a href="LICENSE">
    <img src="https://img.shields.io/github/license/iiiYar/PrayerControllerPro" alt="License" />
  </a>
</p>

---

## Features

- Prayer schedule from AlAdhan API with daily local caching
- City and district support (Saudi Arabia, UAE, Egypt)
- Adhan and Iqama audio playback with configurable file presets
- Media pause and resume automation around prayer times
- Volume guard with smooth fade transitions
- Custom reminders with independent rules per prayer
- Windows system tray with live countdown tooltip
- Windows balloon and Discord webhook notifications
- Auto-start with Windows
- Built-in update checker with version manifest
- Structured local JSONL application logs
- Compact widget mode
- Single-instance enforcement — only one copy runs at a time

## Screenshots

> Screenshots coming soon

## Installation

1. Download the latest release from the [Releases page](https://github.com/iiiYar/PrayerControllerPro/releases)
2. Extract the ZIP file
3. Run `PrayerControllerPro.exe`
4. No installer required

## Requirements

- Windows 10 or Windows 11 (x64)
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

## Build from Source

```bash
git clone https://github.com/iiiYar/PrayerControllerPro.git
cd PrayerControllerPro
dotnet build
dotnet run --project src/PrayerControllerPro.App
```

## Project Structure

```
src/
  PrayerControllerPro.Core/
    Catalogs/         — city, district, calculation method definitions
    Models/           — domain models and enums
    Services/         — prayer time provider, scheduler engine, settings store

  PrayerControllerPro.App/
    Assets/           — icons and embedded resources
    Converters/       — WPF value converters
    Dialogs/
      About/          — About window
      Logs/           — Log viewer window
      Prayer/         — Prayer rule editor window
      Reminder/       — Custom reminder window
      Settings/       — App settings window
      Update/         — Update available window
    Infrastructure/   — ObservableObject base class
    Services/
      Audio/          — Adhan and Iqama playback, preset download
      Logging/        — File-based structured logging
      Notifications/  — Windows tray and Discord notifications
      System/         — Auto-start, tray icon, volume guard, Win32 media
      Updates/        — Update check service and result model
    ViewModels/       — MainViewModel, PrayerCardViewModel

tests/
  PrayerControllerPro.Tests/
```

## Architecture

See [docs/architecture.md](docs/architecture.md) for full data flow and layer responsibilities.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for full version history.

## Contributing

Contributions and bug reports are welcome.

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit your changes with clear messages
4. Open a pull request

## License

Add a license file before publishing for broad reuse.
