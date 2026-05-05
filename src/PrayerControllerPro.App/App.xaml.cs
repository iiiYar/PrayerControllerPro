using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace PrayerControllerPro.App;

public partial class App : System.Windows.Application
{
    private const string SingleInstanceMutexName = @"Local\PrayerControllerPro.SingleInstance";
    private const string ActivateEventName = @"Local\PrayerControllerPro.ActivateEvent";

    private Mutex? _singleInstanceMutex;
    private EventWaitHandle? _activateEvent;
    private CancellationTokenSource? _activationListenerCancellation;
    private bool _ownsSingleInstanceMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        _singleInstanceMutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out _ownsSingleInstanceMutex);
        _activateEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ActivateEventName);

        if (!_ownsSingleInstanceMutex)
        {
            _activateEvent.Set();
            Shutdown();
            return;
        }

        StartActivationListener();

        base.OnStartup(e);
        var window = new MainWindow();
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            _activationListenerCancellation?.Cancel();
            _activateEvent?.Set();

            if (_ownsSingleInstanceMutex)
            {
                _singleInstanceMutex?.ReleaseMutex();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Warning] Single-instance cleanup failed: {ex.Message}");
        }
        finally
        {
            _activationListenerCancellation?.Dispose();
            _activateEvent?.Dispose();
            _singleInstanceMutex?.Dispose();
        }

        base.OnExit(e);
    }

    private void StartActivationListener()
    {
        if (_activateEvent is null)
        {
            return;
        }

        _activationListenerCancellation = new CancellationTokenSource();
        var cancellationToken = _activationListenerCancellation.Token;
        var activateEvent = _activateEvent;

        _ = Task.Run(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!activateEvent.WaitOne(250))
                    {
                        continue;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    Dispatcher.Invoke(ActivateMainWindow);
                }
                catch (Exception ex) when (ex is ObjectDisposedException or InvalidOperationException)
                {
                    Debug.WriteLine($"[Warning] Activation listener stopped: {ex.Message}");
                    return;
                }
            }
        }, cancellationToken);
    }

    private void ActivateMainWindow()
    {
        if (MainWindow is null)
        {
            return;
        }

        MainWindow.Show();
        MainWindow.WindowState = WindowState.Normal;
        MainWindow.Activate();
    }
}
