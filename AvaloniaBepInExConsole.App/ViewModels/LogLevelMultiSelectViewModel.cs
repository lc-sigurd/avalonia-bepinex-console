using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Sigurd.AvaloniaBepInExConsole.App.Collections;
using Sigurd.AvaloniaBepInExConsole.Common;

namespace Sigurd.AvaloniaBepInExConsole.App.ViewModels;

public class LogLevelMultiSelectViewModel : ViewModelBase
{
    public IDictionary<BepInExLogLevel, bool?> LogLevelSelectedStates { get; } = new Dictionary<BepInExLogLevel, bool?> {
        { BepInExLogLevel.Debug, true },
        { BepInExLogLevel.Info, true },
        { BepInExLogLevel.Message, true },
        { BepInExLogLevel.Warning, true },
        { BepInExLogLevel.Error, true },
        { BepInExLogLevel.Fatal, true }
    };

    private readonly MultiLookup<BepInExLogLevel, IObserver<bool?>> _subscribers = new();

    public IObservable<bool?> Listen(BepInExLogLevel key)
    {
        return Observable.Create<bool?>
        (observer =>
        {
            if (LogLevelSelectedStates.TryGetValue(key, out var value))
                observer.OnNext(value);

            _subscribers.Add(key,observer);
            return Disposable.Create(() => _subscribers.Remove(key,observer));
        });
    }

    public bool? DebugSelected {
        get => LogLevelSelectedStates[BepInExLogLevel.Debug];
        set {
            LogLevelSelectedStates[BepInExLogLevel.Debug] = value;
            foreach (var observer in _subscribers[BepInExLogLevel.Debug]) {
                observer.OnNext(value);
            }
        }
    }

    public bool? InfoSelected {
        get => LogLevelSelectedStates[BepInExLogLevel.Info];
        set {
            LogLevelSelectedStates[BepInExLogLevel.Info] = value;
            foreach (var observer in _subscribers[BepInExLogLevel.Info]) {
                observer.OnNext(value);
            }
        }
    }

    public bool? MessageSelected {
        get => LogLevelSelectedStates[BepInExLogLevel.Message];
        set {
            LogLevelSelectedStates[BepInExLogLevel.Message] = value;
            foreach (var observer in _subscribers[BepInExLogLevel.Message]) {
                observer.OnNext(value);
            }
        }
    }

    public bool? WarningSelected {
        get => LogLevelSelectedStates[BepInExLogLevel.Warning];
        set {
            LogLevelSelectedStates[BepInExLogLevel.Warning] = value;
            foreach (var observer in _subscribers[BepInExLogLevel.Warning]) {
                observer.OnNext(value);
            }
        }
    }

    public bool? ErrorSelected {
        get => LogLevelSelectedStates[BepInExLogLevel.Error];
        set {
            LogLevelSelectedStates[BepInExLogLevel.Error] = value;
            foreach (var observer in _subscribers[BepInExLogLevel.Error]) {
                observer.OnNext(value);
            }
        }
    }

    public bool? FatalSelected {
        get => LogLevelSelectedStates[BepInExLogLevel.Fatal];
        set {
            LogLevelSelectedStates[BepInExLogLevel.Fatal] = value;
            foreach (var observer in _subscribers[BepInExLogLevel.Fatal]) {
                observer.OnNext(value);
            }
        }
    }
}
