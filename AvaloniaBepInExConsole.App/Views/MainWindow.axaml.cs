using Avalonia.ReactiveUI;
using AvaloniaBepInExConsole.App.ViewModels;
using ReactiveUI;

namespace AvaloniaBepInExConsole.App.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        this.WhenActivated(_ => { });
        InitializeComponent();
    }
}
