# Architecture

## Solution Overview

PrayerControllerPro is a .NET 8 WPF Windows application.

## Projects

### PrayerControllerPro.Core

No dependency on WPF. Contains:
- Catalogs/ - static lookup data: cities, districts, methods, prayers
- Models/ - all domain models and enums
- Services/ - prayer time provider, schedule composer, scheduler engine,
  settings store, volume transition planner, version comparer

### PrayerControllerPro.App

WPF UI and system integrations. Contains:
- Assets/ - icons and embedded resources
- Converters/ - WPF value converters
- Dialogs/ - dialog windows organized by function
- Infrastructure/ - base classes (ObservableObject)
- Services/ - organized by responsibility:
  - Audio/ - Adhan/Iqama playback, preset download
  - Logging/ - file-based structured JSONL logging
  - Notifications/ - Windows tray balloon and Discord webhook
  - System/ - auto-start, tray icon, volume guard, Win32 media, shared HTTP
  - Updates/ - update manifest check and result model
- ViewModels/ - observable state: MainViewModel, PrayerCardViewModel

### Tests

tests/PrayerControllerPro.Tests/
- AlAdhanPrayerTimeProviderTests
- AppCatalogTests
- SchedulerEngineTests
- SettingsStoreTests
- UpdateVersionComparerTests
- VolumeGuardTransitionPlannerTests

## Data Flow

1. App starts and reads settings.json via SettingsStore.
2. AlAdhanPrayerTimeProvider fetches from api.aladhan.com, or returns today's cached schedule.
3. PrayerScheduleComposer merges prayer times with user rules and custom reminders.
4. SchedulerEngine.Evaluate() is called every second via DispatcherTimer.
5. On trigger:
   - AudioPlaybackService plays Adhan or Iqama.
   - Win32MediaController sends Play/Pause to the system.
   - VolumeGuardService manages volume fade in/out.
   - NotificationService sends Windows balloon or Discord messages.
6. App logs all events via AppLogService using daily JSONL files.
7. UpdateCheckService runs on startup to compare the current version against the update manifest on GitHub.

## Known Limitations

- Win32 media control targets the entire system, not specific processes.
- City and district catalog data is currently hardcoded in AppCatalog.
- No dependency injection container is used; services are composed manually.
- MVVM implementation is partial; MainWindow still holds some orchestration logic.
