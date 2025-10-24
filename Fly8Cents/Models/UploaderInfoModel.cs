using ReactiveUI;

namespace Fly8Cents.Models;

public class UploaderInfoModel : ReactiveObject
{
    private string _avatarUrl = string.Empty;
    private string _nickname = string.Empty;

    private long _uid;

    public long Uid
    {
        get => _uid;
        set => this.RaiseAndSetIfChanged(ref _uid, value);
    }

    public string Nickname
    {
        get => _nickname;
        set => this.RaiseAndSetIfChanged(ref _nickname, value);
    }

    public string AvatarUrl
    {
        get => _avatarUrl;
        set => this.RaiseAndSetIfChanged(ref _avatarUrl, value);
    }
}