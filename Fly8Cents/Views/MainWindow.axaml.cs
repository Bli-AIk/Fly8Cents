using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Fly8Cents.ViewModels;

namespace Fly8Cents.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }
}