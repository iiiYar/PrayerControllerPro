# Project Handoff

This document is the quick starting point for future development sessions.

## Current State

- Current version: `1.2.0`
- Runtime target: `.NET 8`
- UI framework: `WPF`
- Main app project: `src/PrayerControllerPro.App`
- Core logic project: `src/PrayerControllerPro.Core`
- Tests project: `tests/PrayerControllerPro.Tests`
- Local runtime settings: `%AppData%\PrayerControllerPro`
- Published builds: local only under `releases\`, ignored by Git

## Start Here

1. Read `AGENTS.md`.
2. Run `dotnet test tests/PrayerControllerPro.Tests/PrayerControllerPro.Tests.csproj`.
3. Run `dotnet build PrayerControllerPro.sln -c Release`.
4. Use `build.bat` when you need a local published build under `releases\v<version>\win-x64`.

## Important Architecture

- `SchedulerEngine` decides when to pause media, restore media, and play adhan/iqama events.
- `PrayerScheduleComposer` merges API prayer times with custom reminders and user rules.
- `SettingsStore` only reads/writes the current JSON settings file in `%AppData%`.
- `VolumeGuardService` lowers other app audio sessions during prayer windows and can fade volumes down/up in selectable transition styles.
- `UpdateCheckService` reads the public `update.json` manifest and shows users when a newer build is available.
- `Win32MediaController` keeps the compatibility `Play/Pause` key method.
- `MainWindow` is still the main orchestration point and is the best next refactor target.

## Current Media Control Modes

- `PlayPauseKey`: sends the Windows media Play/Pause key.
- `VolumeGuard`: lowers other app volumes during active prayer windows, then restores them with the selected fade style.

## What Was Removed

- The old one-file WinForms app source.
- Root-level legacy JSON settings files.
- Legacy migration code for those removed root-level files.

## Recommended Next Improvements

- Split `MainWindow.xaml.cs` orchestration into smaller app services.
- Add UI tests or smoke tests for opening settings/reminder dialogs.
- Add a safer Windows media-state controller if direct player state becomes necessary.
- Add GitHub Releases if executable builds need to be distributed from GitHub.
