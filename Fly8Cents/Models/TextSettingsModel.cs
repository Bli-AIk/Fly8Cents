using ReactiveUI;

namespace Fly8Cents.Models;

public class TextSettingsModel : ReactiveObject
{
    private string _startText = "8月评论区\n羞辱过我的人";
    private string _endText = "反弹！";

    public string StartText
    {
        get => _startText;
        set
        {
            this.RaiseAndSetIfChanged(ref _startText, value);
            MessageBus.Current.SendMessage(this);
        }
    }

    public string EndText
    {
        get => _endText;
        set
        {
            this.RaiseAndSetIfChanged(ref _endText, value);
            MessageBus.Current.SendMessage(this);
        }
    }
}