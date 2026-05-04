using System.Collections.ObjectModel;
using PrayerControllerPro.App.Infrastructure;

namespace PrayerControllerPro.App.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private string _currentTimeText = "--:--:--";
    private string _currentDateText = "--";
    private string _countdownLabel = "Next prayer";
    private string _countdownPrayerName = "--";
    private string _countdownValue = "--:--:--";
    private string _countdownTargetText = "Adhan at --";
    private string _countdownTargetTimeText = "--";
    private string _countdownMetaText = "Iqama at --";
    private string _statusText = "Loading prayer schedule...";
    private string _sourceText = "--";
    private string _cityText = "--";
    private string _methodText = "--";
    private bool _isBusy;
    private bool _isWidgetMode;
    private string _widgetButtonText = "Widget";

    public ObservableCollection<PrayerCardViewModel> Prayers { get; } = [];

    public string CurrentTimeText
    {
        get => _currentTimeText;
        set => SetProperty(ref _currentTimeText, value);
    }

    public string CurrentDateText
    {
        get => _currentDateText;
        set => SetProperty(ref _currentDateText, value);
    }

    public string CountdownLabel
    {
        get => _countdownLabel;
        set => SetProperty(ref _countdownLabel, value);
    }

    public string CountdownPrayerName
    {
        get => _countdownPrayerName;
        set => SetProperty(ref _countdownPrayerName, value);
    }

    public string CountdownValue
    {
        get => _countdownValue;
        set => SetProperty(ref _countdownValue, value);
    }

    public string CountdownTargetText
    {
        get => _countdownTargetText;
        set => SetProperty(ref _countdownTargetText, value);
    }

    public string CountdownTargetTimeText
    {
        get => _countdownTargetTimeText;
        set => SetProperty(ref _countdownTargetTimeText, value);
    }

    public string CountdownMetaText
    {
        get => _countdownMetaText;
        set => SetProperty(ref _countdownMetaText, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public string SourceText
    {
        get => _sourceText;
        set => SetProperty(ref _sourceText, value);
    }

    public string CityText
    {
        get => _cityText;
        set => SetProperty(ref _cityText, value);
    }

    public string MethodText
    {
        get => _methodText;
        set => SetProperty(ref _methodText, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public bool IsWidgetMode
    {
        get => _isWidgetMode;
        set
        {
            if (SetProperty(ref _isWidgetMode, value))
            {
                WidgetButtonText = value ? "Dashboard" : "Widget";
            }
        }
    }

    public string WidgetButtonText
    {
        get => _widgetButtonText;
        private set => SetProperty(ref _widgetButtonText, value);
    }
}
