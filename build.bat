@echo off
echo ================================================
echo     Prayer Media Controller Pro - Build Script
echo ================================================
echo.

set "CSC_PATH=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"

if not exist "%CSC_PATH%" (
    echo [ERROR] C# Compiler not found at:
    echo %CSC_PATH%
    echo.
    echo Please install .NET Framework Developer Pack.
    pause
    exit /b 1
)

echo [INFO] Compiling PrayerMediaApp_Ultima.cs...
echo.

"%CSC_PATH%" /target:winexe ^
    /out:PrayerControllerPro.exe ^
    /optimize+ ^
    PrayerMediaApp_Ultima.cs ^
    /r:System.Windows.Forms.dll ^
    /r:System.Drawing.dll ^
    /r:System.Web.Extensions.dll ^
    /r:System.dll ^
    /r:Microsoft.CSharp.dll

if %errorlevel% equ 0 (
    echo.
    echo ================================================
    echo [SUCCESS] Build completed successfully!
    echo ================================================
    echo Output: PrayerMediaApp.exe
) else (
    echo.
    echo ================================================
    echo [ERROR] Build failed!
    echo ================================================
)

pause
