using Fly8Cents.Services;
using ReactiveUI;

namespace Fly8Cents.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public BasicInfoViewModel BasicInfo { get; }
    // public FilterViewModel Filter { get; } = new FilterViewModel();
    // public GenerateViewModel Generate { get; } = new GenerateViewModel();
    // public ResultViewModel Result { get; } = new ResultViewModel();

    private int _selectedIndex;
    public MainWindowViewModel()
    {
        var biliService = new BiliService();
        BasicInfo = new BasicInfoViewModel(biliService);
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
    }
}
