using Avalonia.ReactiveUI;
using ReactiveUI;
using Sigurd.AvaloniaBepInExConsole.App.ViewModels;

namespace Sigurd.AvaloniaBepInExConsole.App.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        this.WhenActivated(_ => { });
        InitializeComponent();
    }
}
