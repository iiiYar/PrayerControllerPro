# Architecture

## Solution Overview

PrayerControllerPro is a .NET 8 WPF Windows application split into two focused projects.

---

## Projects

### PrayerControllerPro.Core

No dependency on WPF or any UI framework. Contains all business logic.

| Folder | Responsibility |
|---|---|
| `Catalogs/` | Static lookup data: cities, districts, calculation methods, prayer definitions |
| `Models/` | All domain models and enums used across the solution |
| `Services/` | Prayer time fetching, schedule composition, scheduler engine, settings store, volume planner, version comparer |

### PrayerControllerPro.App

WPF UI layer and all Windows system integrations.

| Folder | Responsibility |
|---|---|
| `Assets/` | Icons and embedded resources |
| `Converters/` | WPF value converters |
| `Dialogs/About/` | About window |
| `Dialogs/Logs/` | Application log viewer window |
| `Dialogs/Prayer/` | Prayer rule editor window |
| `Dialogs/Reminder/` | Custom reminder window |
| `Dialogs/Settings/` | App settings window |
| `Dialogs/Update/` | Update available window |
| `Infrastructure/` | Base classes (ObservableObject) |
| `Services/Audio/` | Adhan and Iqama playback, preset download |
| `Services/Logging/` | File-based structured JSONL logging |
| `Services/Notifications/` | Windows tray balloon and Discord webhook notifications |
| `Services/System/` | Auto-start, tray icon, volume guard, Win32 media control |
| `Services/Updates/` | Update manifest check and result model |
| `ViewModels/` | Observable state: MainViewModel, PrayerCardViewModel |

### Tests

`tests/PrayerControllerPro.Tests/` — unit tests for core services and engine logic:
- `AlAdhanPrayerTimeProviderTests`
- `AppCatalogTests`
- `SchedulerEngineTests`
- `SettingsStoreTests`
- `UpdateVersionComparerTests`
- `VolumeGuardTransitionPlannerTests`

---

## Data Flow

```
1. App starts
   └─ reads settings.json via SettingsStore

2. AlAdhanPrayerTimeProvider
   └─ fetches prayer times from api.aladhan.com
   └─ or returns today's cached schedule if already fetched

3. PrayerScheduleComposer
   └─ merges raw prayer times with user rules and custom reminders
   └─ produces a flat list of scheduled triggers for the day

4. SchedulerEngine.Evaluate()
   └─ called every second via DispatcherTimer
   └─ compares current time against scheduled triggers

5. On trigger:
   ├─ AudioPlaybackService     → plays Adhan or Iqama audio file
   ├─ Win32MediaController     → sends Play/Pause to system media
   ├─ VolumeGuardService       → manages volume fade in/out transitions
   └─ NotificationService      → sends Windows balloon or Discord webhook

6. AppLogService
   └─ logs all events to daily JSONL files in AppData

7. UpdateCheckService
   └─ runs on startup
   └─ compares current version against update.json manifest on GitHub
   └─ shows UpdateAvailableWindow if a newer version exists
```

---

## Single Instance

The application enforces a single running instance using:
- A named `Mutex`: `Local\PrayerControllerPro.SingleInstance`
- A named `EventWaitHandle`: `Local\PrayerControllerPro.ActivateEvent`

If a second instance is launched:
1. It detects the mutex is already owned
2. It signals the activation event
3. It exits immediately
4. The first instance receives the signal and brings its window to the foreground

---

## Known Limitations

- Win32 media control targets the entire system, not a specific process
- City and district catalog is currently hardcoded in `AppCatalog`
- No dependency injection container — services are composed manually in `App.xaml.cs`
- MVVM implementation is partial — `MainWindow.xaml.cs` still holds some orchestration logic
