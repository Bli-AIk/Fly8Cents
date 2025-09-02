using System.Net.Http;
using ReactiveUI;

namespace Fly8Cents.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private int _selectedIndex;

    public MainWindowViewModel()
    {
        HttpClient = GetHttpClient();

        QrLogin = new QrLoginViewModel(HttpClient);
        BasicInfo = new BasicInfoViewModel(HttpClient);
    }

    public ConfigViewModel Config { get; } = new();
    public TextSettingsViewModel TextSettings { get; } = new();
    public ExportViewModel Export { get; } = new();

    private HttpClient HttpClient { get; }
    public QrLoginViewModel QrLogin { get; }
    public BasicInfoViewModel BasicInfo { get; }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
    }

    public static HttpClient GetHttpClient(HttpClientHandler? handler = null)
    {
        var httpClient = handler != null ? new HttpClient(handler) : new HttpClient();

        httpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        httpClient.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
        httpClient.DefaultRequestHeaders.Add("Referer", "https://www.bilibili.com/");

        return httpClient;
    }
}