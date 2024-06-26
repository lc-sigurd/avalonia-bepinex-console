using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using Sigurd.AvaloniaBepInExConsole.App.Logs;
using Sigurd.AvaloniaBepInExConsole.Common;

namespace Sigurd.AvaloniaBepInExConsole.App.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly ReadOnlyObservableCollection<LogEvent> _logMessages;
    public ReadOnlyObservableCollection<LogEvent> LogMessages => _logMessages;

    public MainWindowViewModel(ILogListener logListener)
    {
        LogLevelMultiSelect = new();

        logListener.LogMessages.Connect()
            .FilterOnObservable(logEvent => {
                return LogLevelMultiSelect.Listen(logEvent.Level).Select(selected => {
                    return selected ?? false;
                });
            })
            .Bind(out _logMessages)
            .Subscribe();
    }

    public LogLevelMultiSelectViewModel LogLevelMultiSelect { get; }
}
