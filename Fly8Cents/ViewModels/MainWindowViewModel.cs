using Fly8Cents.Services;
using ReactiveUI;

namespace Fly8Cents.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public string SessData = "";
    public QrLoginViewModel QrLogin { get; }
    public BasicInfoViewModel BasicInfo { get; } = new();
    // public FilterViewModel Filter { get; } = new FilterViewModel();
    // public GenerateViewModel Generate { get; } = new GenerateViewModel();
    // public ResultViewModel Result { get; } = new ResultViewModel();

    private int _selectedIndex;
    public MainWindowViewModel()
    {
        QrLogin = new QrLoginViewModel(this);
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
    }
}
