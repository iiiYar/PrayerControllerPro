# Changelog

All notable changes to this project are documented in this file.
Format based on [Keep a Changelog](https://keepachangelog.com).

## [Unreleased]

## [1.3.0] - 2026-05-05

### Added
- Prepared the first official GitHub Release package and update manifest flow
- Added Iqama catch-up scheduler coverage to match Adhan wake/startup behavior

### Changed
- Reorganized `Services/` into `Audio/`, `Logging/`, `Notifications/`, `System/`, `Updates/` subfolders
- Renamed `State/` to `ViewModels/` with proper ViewModel naming conventions
  - `MainViewState` → `MainViewModel`
  - `PrayerCardState` → `PrayerCardViewModel`
- Split `Dialogs/` into per-dialog subfolders (`About/`, `Logs/`, `Prayer/`, `Reminder/`, `Settings/`, `Update/`)
- Updated all namespaces and type references throughout the codebase
- Updated NuGet dependencies: NAudio 2.3.0, Microsoft.NET.Test.Sdk 18.5.1, xunit 2.9.3, xunit.runner.visualstudio 3.1.5, and coverlet.collector 10.0.0
- Made VolumeGuard the true default media control mode for new installs

### Fixed
- Restored guarded audio sessions synchronously on app exit
- Reduced the chance of a canceled VolumeGuard fade writing volume after immediate restore
- Extended Adhan and Iqama audio catch-up to five minutes after wake/startup
- Single-instance enforcement via named `Mutex` + `EventWaitHandle`
  — launching the app twice now activates the existing window instead of opening a second instance
- Updated the project handoff version to match the application version

## [1.2.1] - 2026-04-30

### Changed
- Applied official Prayer Controller Pro identity across repo and app
- Added branded app icon and tray icon improvements
- Improved About window with version, publisher, repository, and update feed details
- Updated release metadata and repository presentation

## [1.2.0] - 2026-04-30

### Added
- Expanded audio playback and notification functionality

### Changed
- Continued transition toward structured application layout

## [1.1.1] - 2026-04-30

### Fixed
- Bug fixes and stability improvements for scheduling and WPF layer

## [1.1.0] - 2026-04-27

### Added
- Improved prayer scheduling engine
- Expanded core logic and automation behavior

### Changed
- Repository structure and release packaging improvements

## [1.0.11] - 2026-04-27

### Changed
- Cleaned repository for release consistency
- Reduced codebase clutter

## [1.0.10] - 2026-04-27

### Added
- WPF application release line

### Changed
- Significant application-level rewrite and packaging

## [1.0.2] - 2026-02-18

### Added
- Initial public release
- Custom prayer management with JSON-based configuration
- Early WPF UI and automation foundation

[1.3.0]: https://github.com/iiiYar/PrayerControllerPro/releases/tag/v1.3.0
[1.2.1]: https://github.com/iiiYar/PrayerControllerPro/releases/tag/v1.2.1
[1.2.0]: https://github.com/iiiYar/PrayerControllerPro/releases/tag/v1.2.0
[1.1.1]: https://github.com/iiiYar/PrayerControllerPro/releases/tag/v1.1.1
[1.1.0]: https://github.com/iiiYar/PrayerControllerPro/releases/tag/v1.1.0
[1.0.11]: https://github.com/iiiYar/PrayerControllerPro/releases/tag/v1.0.11
[1.0.10]: https://github.com/iiiYar/PrayerControllerPro/releases/tag/v1.0.10
[1.0.2]: https://github.com/iiiYar/PrayerControllerPro/releases/tag/v1.0.2
