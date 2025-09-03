using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;

namespace Fly8Cents.ViewModels;

public class ConfigViewModel : ViewModelBase
{
    private ObservableCollection<string> _blackList = [];

    private string _newBlackItem = "";

    private string _newWhiteItem = "";
    private string? _selectedBlackItem;
    private string? _selectedWhiteItem;

    private ObservableCollection<string> _whiteList = [];

    public ConfigViewModel()
    {
        AddBlackCommand = ReactiveCommand.Create(() =>
        {
            if (string.IsNullOrWhiteSpace(NewBlackItem))
            {
                return;
            }

            if (BlackList.Any(item => item == NewWhiteItem))
            {
                return;
            }

            BlackList.Add(NewBlackItem);
            NewBlackItem = "";
        });

        RemoveBlackCommand = ReactiveCommand.Create(() =>
        {
            if (!string.IsNullOrWhiteSpace(SelectedBlackItem))
            {
                BlackList.Remove(SelectedBlackItem);
            }
        });

        ClearBlackCommand = ReactiveCommand.Create(() =>
        {
            BlackList.Clear();
        });
        
        AddWhiteCommand = ReactiveCommand.Create(() =>
        {
            if (string.IsNullOrWhiteSpace(NewWhiteItem))
            {
                return;
            }

            if (WhiteList.Any(item => item == NewWhiteItem))
            {
                return;
            }

            WhiteList.Add(NewWhiteItem);
            NewWhiteItem = "";
        });

        RemoveWhiteCommand = ReactiveCommand.Create(() =>
        {
            if (!string.IsNullOrWhiteSpace(SelectedWhiteItem))
            {
                WhiteList.Remove(SelectedWhiteItem);
            }
        });
        
        ClearWhiteCommand = ReactiveCommand.Create(() =>
        {
            WhiteList.Clear();
        });
    }

    public ObservableCollection<string> BlackList
    {
        get => _blackList;
        set => this.RaiseAndSetIfChanged(ref _blackList, value);
    }

    public string NewBlackItem
    {
        get => _newBlackItem;
        set => this.RaiseAndSetIfChanged(ref _newBlackItem, value);
    }

    public ObservableCollection<string> WhiteList
    {
        get => _whiteList;
        set => this.RaiseAndSetIfChanged(ref _whiteList, value);
    }

    public string NewWhiteItem
    {
        get => _newWhiteItem;
        set => this.RaiseAndSetIfChanged(ref _newWhiteItem, value);
    }

    public string? SelectedWhiteItem
    {
        get => _selectedWhiteItem;
        set => this.RaiseAndSetIfChanged(ref _selectedWhiteItem, value);
    }

    public string? SelectedBlackItem
    {
        get => _selectedBlackItem;
        set => this.RaiseAndSetIfChanged(ref _selectedBlackItem, value);
    }

    private bool _isWhiteHomonym;
    public bool IsWhiteHomonym
    {
        get => _isWhiteHomonym;
        set => this.RaiseAndSetIfChanged(ref _isWhiteHomonym, value);
    }

    private bool _isBlackHomonym;
    public bool IsBlackHomonym
    {
        get => _isBlackHomonym;
        set => this.RaiseAndSetIfChanged(ref _isBlackHomonym, value);
    }
    
    public ReactiveCommand<Unit, Unit> AddBlackCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveBlackCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearBlackCommand { get; }
    public ReactiveCommand<Unit, Unit> AddWhiteCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveWhiteCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearWhiteCommand { get; }
}
