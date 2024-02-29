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

    public MainWindowViewModel(ILogListener logListener)
    {
        logListener.LogMessages.Connect()
            .Bind(out _logMessages)
            .Subscribe();
    }
}
