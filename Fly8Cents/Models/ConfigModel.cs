using System.Collections.ObjectModel;
using ReactiveUI;

namespace Fly8Cents.Models;

public class ConfigModel : ReactiveObject
{
    private ObservableCollection<string> _blackList = [];
    private bool _isBlackHomonym;
    private bool _isWhiteHomonym;
    private ObservableCollection<string> _whiteList = [];
    private bool _isUseAi = false;
    private string _aiPrompt = "";
    private string _aiEndpoint = "";
    private string _aiApiKey = "";

    public ObservableCollection<string> BlackList
    {
        get => _blackList;
        set => this.RaiseAndSetIfChanged(ref _blackList, value);
    }

    public ObservableCollection<string> WhiteList
    {
        get => _whiteList;
        set => this.RaiseAndSetIfChanged(ref _whiteList, value);
    }

    public bool IsWhiteHomonym
    {
        get => _isWhiteHomonym;
        set => this.RaiseAndSetIfChanged(ref _isWhiteHomonym, value);
    }

    public bool IsBlackHomonym
    {
        get => _isBlackHomonym;
        set => this.RaiseAndSetIfChanged(ref _isBlackHomonym, value);
    }
    
    public bool IsUseAi
    {
        get => _isUseAi;
        set => this.RaiseAndSetIfChanged(ref _isUseAi, value);
    }
    
    public string AiPrompt
    {
        get => _aiPrompt;
        set => this.RaiseAndSetIfChanged(ref _aiPrompt, value);
    }
    
    public string AiEndpoint
    {
        get => _aiEndpoint;
        set => this.RaiseAndSetIfChanged(ref _aiEndpoint, value);
    }
    
    public string AiApiKey
    {
        get => _aiApiKey;
        set => this.RaiseAndSetIfChanged(ref _aiApiKey, value);
    }
}