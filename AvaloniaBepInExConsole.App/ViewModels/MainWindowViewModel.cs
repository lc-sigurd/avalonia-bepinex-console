using System;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;
using Sigurd.AvaloniaBepInExConsole.App.Logs;
using Sigurd.AvaloniaBepInExConsole.Common;

namespace Sigurd.AvaloniaBepInExConsole.App.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly ReadOnlyObservableCollection<LogEvent> _logMessages;
    public ReadOnlyObservableCollection<LogEvent> LogMessages => _logMessages;

    public ObservableCollection<LogEvent> TestLogMessages { get; } = new(Enumerable.Repeat(new LogEvent { Data = "\x1b[0;38;5;99mthe quick bro\x1b[0mwn fox jumps over \x1b[107;94mthe lazy dog 0123456789", Level = BepInExLogLevel.Info, SourceName = "Foobar"}, 150));

    public MainWindowViewModel(BepInExLogListener logListener)
    {
        logListener.LogMessages.Connect()
            .Bind(out _logMessages)
            .Subscribe();
    }
}
