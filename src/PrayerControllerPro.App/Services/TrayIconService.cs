using System.Drawing;
using Forms = System.Windows.Forms;

namespace PrayerControllerPro.App.Services;

public sealed class TrayIconService : IDisposable
{
    private readonly Forms.NotifyIcon _notifyIcon = new();

    public void Initialize(Action onOpenRequested, Action onExitRequested)
    {
        _notifyIcon.Icon = SystemIcons.Application;
        _notifyIcon.Visible = true;
        _notifyIcon.Text = "Prayer Controller Pro";

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Open", null, (_, _) => onOpenRequested());
        menu.Items.Add("-");
        menu.Items.Add("Exit", null, (_, _) => onExitRequested());
        _notifyIcon.ContextMenuStrip = menu;
        _notifyIcon.DoubleClick += (_, _) => onOpenRequested();
    }

    public void UpdateTooltip(string cityName, string prayerName, string countdown)
    {
        var text = $"Prayer Controller | {cityName} | {prayerName} | {countdown}";
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
    }
}
