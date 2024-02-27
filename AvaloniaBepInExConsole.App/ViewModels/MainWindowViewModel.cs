using System;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;
using Sigurd.AvaloniaBepInExConsole.App.Logs;

namespace Sigurd.AvaloniaBepInExConsole.App.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly ReadOnlyObservableCollection<LogMessage> _logMessages;
    public ReadOnlyObservableCollection<LogMessage> LogMessages => _logMessages;

    public ObservableCollection<LogMessage> TestLogMessages { get; } = new(Enumerable.Repeat(new LogMessage("\x1b[0;38;5;99mthe quick bro\x1b[0mwn fox jumps over \x1b[107;94mthe lazy dog 0123456789"), 150));

    public MainWindowViewModel(BepInExLogListener logListener)
    {
        logListener.LogMessages.Connect()
            .Bind(out _logMessages)
            .Subscribe();
    }
}
