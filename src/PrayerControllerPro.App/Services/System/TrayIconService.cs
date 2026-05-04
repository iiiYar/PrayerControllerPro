using System.Drawing;
using Forms = System.Windows.Forms;

namespace PrayerControllerPro.App.Services.System;

public sealed class TrayIconService : IDisposable
{
    private readonly Forms.NotifyIcon _notifyIcon = new();
    private Icon? _appIcon;

    public void Initialize(Action onOpenRequested, Action onExitRequested)
    {
        _appIcon = LoadAppIcon();
        _notifyIcon.Icon = _appIcon ?? SystemIcons.Application;
        _notifyIcon.Visible = true;
        _notifyIcon.Text = AppIdentity.ProductName;

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Open", null, (_, _) => onOpenRequested());
        menu.Items.Add("-");
        menu.Items.Add("Exit", null, (_, _) => onExitRequested());
        _notifyIcon.ContextMenuStrip = menu;
        _notifyIcon.DoubleClick += (_, _) => onOpenRequested();
    }

    public void UpdateTooltip(string cityName, string prayerName, string countdown)
    {
        var text = $"Prayer Controller Pro | {cityName} | {prayerName} | {countdown}";
        _notifyIcon.Text = text.Length > 63 ? text[..63] : text;
    }

    public void ShowMessage(string title, string message)
    {
        _notifyIcon.BalloonTipTitle = title;
        _notifyIcon.BalloonTipText = message;
        _notifyIcon.ShowBalloonTip(3000);
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _appIcon?.Dispose();
    }

    private static Icon? LoadAppIcon()
    {
        var resource = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Assets/app.ico"));
        if (resource is null)
        {
            return null;
        }

        using var stream = resource.Stream;
        using var icon = new Icon(stream);
        return (Icon)icon.Clone();
    }
}
