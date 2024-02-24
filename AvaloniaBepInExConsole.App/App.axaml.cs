using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sigurd.AvaloniaBepInExConsole.App.Logs;
using Sigurd.AvaloniaBepInExConsole.App.ViewModels;
using MainWindow = Sigurd.AvaloniaBepInExConsole.App.Views.MainWindow;

namespace Sigurd.AvaloniaBepInExConsole.App;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {

            var cts = new CancellationTokenSource();

            desktop.ShutdownRequested += (sender, args) => cts.Cancel();

            var logListener = new BepInExLogListener();

            Task.Run(async () => await logListener.StartAsync(cts.Token), cts.Token);

            desktop.MainWindow = new MainWindow {
                DataContext = new MainWindowViewModel(logListener),
            };
        }


        base.OnFrameworkInitializationCompleted();
    }
}
