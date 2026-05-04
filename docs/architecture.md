# Architecture

## Solution Overview

PrayerControllerPro is a Windows desktop application built with .NET 8 and WPF.
The solution is divided into two projects:

### PrayerControllerPro.Core
Contains all business logic with no dependency on WPF or UI frameworks.

Folders:
- Catalogs/   — static lookup data: cities, districts, calculation methods, prayer definitions
- Models/     — all domain models and enums used across the solution
- Services/   — prayer time fetching, scheduling engine, settings store, volume planner, version comparer

### PrayerControllerPro.App
Contains the WPF UI layer and all system integrations.

Folders:
- Assets/         — icons and embedded resources
- Converters/     — WPF value converters
- Dialogs/        — dialog windows, each in its own subfolder
- Infrastructure/ — base classes (ObservableObject)
- Services/       — organized by function:
    Audio/         — Adhan/Iqama playback and preset download
    Logging/       — structured file-based logging
    Notifications/ — Windows tray and Discord notifications
    System/        — auto-start, tray icon, volume guard, Win32 media control
    Updates/       — update check and result models
- ViewModels/     — observable state for MainWindow and prayer cards

### Tests
tests/PrayerControllerPro.Tests/ — unit tests for core services and engine logic

## Data Flow

1. App starts → reads settings.json via SettingsStore
2. AlAdhanPrayerTimeProvider fetches times from api.aladhan.com (or returns cached)
3. PrayerScheduleComposer merges times with user rules and custom reminders
4. SchedulerEngine.Evaluate() runs every second via DispatcherTimer
5. On trigger → AudioPlaybackService plays Adhan/Iqama
                Win32MediaController pauses/resumes media
                VolumeGuardService manages volume transitions
                NotificationService sends Windows/Discord notifications
