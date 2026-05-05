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

6. Create a GitHub Release named `vX.Y.Z` and upload the ZIP as a release asset.

7. Update `update.json` to point at the official GitHub Release asset:

```json
{
  "latestVersion": "X.Y.Z",
  "title": "Prayer Controller Pro vX.Y.Z",
  "notes": "Short release notes shown inside the app.",
  "downloadUrl": "https://github.com/iiiYar/PrayerControllerPro/releases/download/vX.Y.Z/PrayerControllerPro-vX.Y.Z-win-x64.zip",
  "releaseUrl": "https://github.com/iiiYar/PrayerControllerPro/releases/tag/vX.Y.Z",
  "sha256": "ZIP_SHA256_HERE",
  "mandatory": false
}
```

8. Commit the version bump, docs, and `update.json`.
9. Create and push an annotated Git tag named `vX.Y.Z`.

Users on older versions will see the update dialog the next time the app starts, unless they disabled automatic update checks.
