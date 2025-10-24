using Fly8Cents.Models;
using ReactiveUI;

namespace Fly8Cents.ViewModels;

public class TextSettingsViewModel : ViewModelBase
{
    private TextSettingsModel _textSettings = new();
    public TextSettingsModel TextSettings
    {
        get => _textSettings;
        set => this.RaiseAndSetIfChanged(ref _textSettings, value);
    }

    public TextSettingsViewModel()
    {
        MessageBus.Current.SendMessage(TextSettings);
    }
}