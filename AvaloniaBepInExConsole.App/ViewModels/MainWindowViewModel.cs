using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using AvaloniaBepInExConsole.App.Logs;
using DynamicData;
using ReactiveUI;

namespace AvaloniaBepInExConsole.App.ViewModels;

public class MainWindowViewModel : ViewModelBase, IActivatableViewModel
{
    public ViewModelActivator Activator { get; }

    private ReadOnlyObservableCollection<LogMessage>? _logMessages;
    public ReadOnlyObservableCollection<LogMessage>? LogMessages => _logMessages;

    public ObservableCollection<LogMessage> TestLogMessages { get; } = new(Enumerable.Repeat(new LogMessage("the quick brown fox jumps over the lazy dog 0123456789"), 150));

    public MainWindowViewModel()
    {
        Activator = new ViewModelActivator();

        this.WhenActivated(disposables => {

            var logListener = new BepInExLogListener()
                .DisposeWith(disposables);

            logListener.LogMessages.Connect()
                .Bind(out _logMessages)
                .Subscribe();

        });
    }
}
