using System.Reactive;
using Fly8Cents.Models;
using ReactiveUI;

namespace Fly8Cents.ViewModels;

public class ConfigViewModel : ViewModelBase
{
    private ConfigModel _config = new();
    private bool _isUseAi;

    private string _newBlackItem = "";
    private string _newWhiteItem = "";
    private string? _selectedBlackItem;
    private string? _selectedWhiteItem;

    public ConfigViewModel()
    {
        AddBlackCommand = ReactiveCommand.Create(() =>
        {
            if (string.IsNullOrWhiteSpace(NewBlackItem) || Config.BlackList.Contains(NewBlackItem))
            {
                return;
            }

            Config.BlackList.Add(NewBlackItem);
            NewBlackItem = "";
            MessageBus.Current.SendMessage(Config);
        });

        RemoveBlackCommand = ReactiveCommand.Create(() =>
        {
            if (string.IsNullOrWhiteSpace(SelectedBlackItem))
            {
                return;
            }

            Config.BlackList.Remove(SelectedBlackItem);
            MessageBus.Current.SendMessage(Config);
        });

        ClearBlackCommand = ReactiveCommand.Create(() =>
        {
            Config.BlackList.Clear();
            MessageBus.Current.SendMessage(Config);
        });

        AddWhiteCommand = ReactiveCommand.Create(() =>
        {
            if (string.IsNullOrWhiteSpace(NewWhiteItem) || Config.WhiteList.Contains(NewWhiteItem))
            {
                return;
            }

            Config.WhiteList.Add(NewWhiteItem);
            NewWhiteItem = "";
            MessageBus.Current.SendMessage(Config);
        });

        RemoveWhiteCommand = ReactiveCommand.Create(() =>
        {
            if (string.IsNullOrWhiteSpace(SelectedWhiteItem))
            {
                return;
            }

            Config.WhiteList.Remove(SelectedWhiteItem);
            MessageBus.Current.SendMessage(Config);
        });

        ClearWhiteCommand = ReactiveCommand.Create(() =>
        {
            Config.WhiteList.Clear();
            MessageBus.Current.SendMessage(Config);
        });
    }

    public ConfigModel Config
    {
        get => _config;
        set => this.RaiseAndSetIfChanged(ref _config, value);
    }

    public string NewBlackItem
    {
        get => _newBlackItem;
        set => this.RaiseAndSetIfChanged(ref _newBlackItem, value);
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

    public bool IsUseAi
    {
        get => _isUseAi;
        set
        {
            if (Config.IsUseAi == value)
            {
                return;
            }

            Config.IsUseAi = value;
            this.RaiseAndSetIfChanged(ref _isUseAi, value);
            MessageBus.Current.SendMessage(Config);
        }
    }

    public string AiEndpoint
    {
        get => Config.AiEndpoint;
        set
        {
            if (Config.AiEndpoint == value)
            {
                return;
            }

            Config.AiEndpoint = value;
            MessageBus.Current.SendMessage(Config);
        }
    }

    public string AiApiKey
    {
        get => Config.AiApiKey;
        set
        {
            if (Config.AiApiKey == value)
            {
                return;
            }

            Config.AiApiKey = value;
            MessageBus.Current.SendMessage(Config);
        }
    }

    public string AiModel
    {
        get => Config.AiModel;
        set
        {
            if (Config.AiModel == value)
            {
                return;
            }

            Config.AiModel = value;
            MessageBus.Current.SendMessage(Config);
        }
    }

    public string AiPrompt
    {
        get => Config.AiPrompt;
        set
        {
            if (Config.AiPrompt == value)
            {
                return;
            }

            Config.AiPrompt = value;
            MessageBus.Current.SendMessage(Config);
        }
    }

    public ReactiveCommand<Unit, Unit> AddBlackCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveBlackCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearBlackCommand { get; }
    public ReactiveCommand<Unit, Unit> AddWhiteCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveWhiteCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearWhiteCommand { get; }
}