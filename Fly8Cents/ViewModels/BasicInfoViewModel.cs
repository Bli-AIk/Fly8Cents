using System;
using System.Reactive;
using Fly8Cents.Services;
using ReactiveUI;

namespace Fly8Cents.ViewModels;

public class BasicInfoViewModel : ViewModelBase
{
    private DateTimeOffset _endDate = DateTimeOffset.Now;

    private DateTimeOffset _startDate = DateTimeOffset.Now.AddDays(-7);
    private string _uid = "1424609716";

    public string Uid
    {
        get => _uid;
        set => this.RaiseAndSetIfChanged(ref _uid, value);
    }


    private string _uploaderAvatarUrl = "";

    private string _uploaderNickname = "";

    public BasicInfoViewModel(BiliService biliService)
    {
        CheckUid = ReactiveCommand.CreateFromTask(async () =>
        {
            var userCard = await biliService.UserService.GetUserInformationAsync(Uid);
            if (userCard.Profile != null)
            {
                UploaderNickname = userCard.Profile.User.Id;
                UploaderAvatarUrl = userCard.Profile.User.Avatar?.SourceUri.ToString() ?? string.Empty;
            }

            Console.WriteLine($"检查 UID: {Uid}, 时间范围: {StartDate:d} - {EndDate:d}");
        });
    }

    public DateTimeOffset StartDate
    {
        get => _startDate;
        set => this.RaiseAndSetIfChanged(ref _startDate, value);
    }

    public DateTimeOffset EndDate
    {
        get => _endDate;
        set => this.RaiseAndSetIfChanged(ref _endDate, value);
    }

    public ReactiveCommand<Unit, Unit> CheckUid { get; }

    public string UploaderAvatarUrl
    {
        get => _uploaderAvatarUrl;
        set => this.RaiseAndSetIfChanged(ref _uploaderAvatarUrl, value);
    }

    public string UploaderNickname
    {
        get => _uploaderNickname;
        set => this.RaiseAndSetIfChanged(ref _uploaderNickname, value);
    }
}