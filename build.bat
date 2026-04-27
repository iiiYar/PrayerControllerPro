@echo off
setlocal
set "APP_PROJECT=src\PrayerControllerPro.App\PrayerControllerPro.App.csproj"
set "RUNTIME_ID=win-x64"

echo ================================================
echo        Prayer Controller Pro - .NET 8 Build
echo ================================================
echo.

where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] dotnet SDK was not found.
    echo Please install .NET 8 SDK first.
    exit /b 1
)

echo [INFO] Restoring packages...
dotnet restore PrayerControllerPro.sln
if %errorlevel% neq 0 exit /b %errorlevel%

for /f "usebackq delims=" %%i in (`dotnet msbuild "%APP_PROJECT%" -nologo -getProperty:Version`) do set "APP_VERSION=%%i"
if not defined APP_VERSION (
    echo [ERROR] Could not read the application version.
    exit /b 1
)

set "OUTPUT_DIR=releases\v%APP_VERSION%\%RUNTIME_ID%"

echo [INFO] Current version: %APP_VERSION%

echo.
echo [INFO] Building solution...
dotnet build PrayerControllerPro.sln -c Release --no-restore
if %errorlevel% neq 0 exit /b %errorlevel%

echo.
echo [INFO] Publishing app...
dotnet publish "%APP_PROJECT%" -c Release -r %RUNTIME_ID% --self-contained false -p:PublishSingleFile=false -o "%OUTPUT_DIR%"
if %errorlevel% neq 0 exit /b %errorlevel%

(
    echo Product: Prayer Controller Pro
    echo Version: %APP_VERSION%
    echo Runtime: %RUNTIME_ID%
    echo Output: %OUTPUT_DIR%
    echo Built: %DATE% %TIME%
) > "%OUTPUT_DIR%\version.txt"

echo.
echo [SUCCESS] Build completed.
echo Output:
echo   %OUTPUT_DIR%
echo Version file:
echo   %OUTPUT_DIR%\version.txt
endlocal
