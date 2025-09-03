using System.Collections.ObjectModel;
using ReactiveUI;

namespace Fly8Cents.Models;

public class ConfigModel : ReactiveObject
{
    private ObservableCollection<string> _blackList = [];
    private bool _isBlackHomonym;
    private bool _isWhiteHomonym;
    private ObservableCollection<string> _whiteList = [];

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
}