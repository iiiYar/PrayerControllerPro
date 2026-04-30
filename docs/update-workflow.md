# Update Workflow

The app checks this public manifest on startup:

```text
https://raw.githubusercontent.com/iiiYar/PrayerControllerPro/main/update.json
```

## Release Steps

1. Bump the version in `Directory.Build.props`.
2. Update `docs/whats-new.md`, `docs/release-history.md`, and `README.md`.
3. Run:

```powershell
dotnet test tests\PrayerControllerPro.Tests\PrayerControllerPro.Tests.csproj --configuration Release
.\build.bat
```

4. Create a ZIP from the published folder:

```powershell
Compress-Archive -Path releases\vX.Y.Z\win-x64\* -DestinationPath releases\PrayerControllerPro-vX.Y.Z-win-x64.zip -CompressionLevel Optimal
```

5. Calculate the ZIP hash:

```powershell
Get-FileHash releases\PrayerControllerPro-vX.Y.Z-win-x64.zip -Algorithm SHA256
```

6. Copy the ZIP into `downloads\` so the app can download it from GitHub Raw:

```powershell
New-Item -ItemType Directory -Force downloads
Copy-Item releases\PrayerControllerPro-vX.Y.Z-win-x64.zip downloads\PrayerControllerPro-vX.Y.Z-win-x64.zip
```

7. Update `update.json`:

```json
{
  "latestVersion": "X.Y.Z",
  "title": "Prayer Controller Pro vX.Y.Z",
  "notes": "Short release notes shown inside the app.",
  "downloadUrl": "https://raw.githubusercontent.com/iiiYar/PrayerControllerPro/main/downloads/PrayerControllerPro-vX.Y.Z-win-x64.zip",
  "releaseUrl": "https://github.com/iiiYar/PrayerControllerPro/tree/vX.Y.Z",
  "sha256": "ZIP_SHA256_HERE",
  "mandatory": false
}
```

8. Commit the version bump, docs, `update.json`, and the `downloads\` ZIP.
9. Create and push an annotated Git tag named `vX.Y.Z`.

Users on older versions will see the update dialog the next time the app starts, unless they disabled automatic update checks.
