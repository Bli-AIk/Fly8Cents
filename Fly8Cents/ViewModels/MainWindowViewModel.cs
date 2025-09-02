using System.Net.Http;
using Fly8Cents.Services;
using ReactiveUI;

namespace Fly8Cents.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private HttpClient HttpClient { get; set; }
    public QrLoginViewModel QrLogin { get; }
    public BasicInfoViewModel BasicInfo { get; }
    // public FilterViewModel Filter { get; } = new FilterViewModel();
    // public GenerateViewModel Generate { get; } = new GenerateViewModel();
    // public ResultViewModel Result { get; } = new ResultViewModel();

    private int _selectedIndex;
    public MainWindowViewModel()
    {
        HttpClient = new HttpClient();
        QrLogin = new QrLoginViewModel(HttpClient);
        BasicInfo = new BasicInfoViewModel(HttpClient);
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
    }
}
