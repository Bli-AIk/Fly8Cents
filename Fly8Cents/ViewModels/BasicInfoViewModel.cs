using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Fly8Cents.Services;
using Newtonsoft.Json.Linq;
using QuickType.UserSpaceDetails;
using ReactiveUI;

namespace Fly8Cents.ViewModels;

public class BasicInfoViewModel : ViewModelBase
{
    private DateTimeOffset _endDate = DateTimeOffset.Now;

    private DateTimeOffset _startDate = DateTimeOffset.Now.AddDays(-7);
    private long _uid = 1424609716;

    private Bitmap _uploaderAvatar;

    private string _uploaderNickname = "请输入文本";

    public BasicInfoViewModel(HttpClient httpClient)
    {
        _uploaderAvatar = GetDefaultBitmap();
        CheckUid = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                var data = await BiliService.GetUserSpaceDetailsData(httpClient, _uid);
                if (data != null)
                {
                    UploaderNickname = data.Data.Name;
                    UploaderAvatar = await SetImageFromUrl(data.Data.Face);
                }
                else
                {
                    UploaderNickname = "风控校验失败，请登录后重试";
                    UploaderAvatar = GetDefaultBitmap();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"CheckUid报错：{e}");
            }
        });
    }

    public long Uid
    {
        get => _uid;
        set => this.RaiseAndSetIfChanged(ref _uid, value);
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

    public Bitmap UploaderAvatar
    {
        get => _uploaderAvatar;
        set => this.RaiseAndSetIfChanged(ref _uploaderAvatar, value);
    }

    public string UploaderNickname
    {
        get => _uploaderNickname;
        set => this.RaiseAndSetIfChanged(ref _uploaderNickname, value);
    }

    private static Bitmap GetDefaultBitmap()
    {
        return new Bitmap(AssetLoader.Open(new Uri("avares://Fly8Cents/Assets/default.jpg")));
    }

    private static async Task<Bitmap> SetImageFromUrl(Uri url)
    {
        using var httpClient = new HttpClient();
        var bytes = await httpClient.GetByteArrayAsync(url);
        using var stream = new MemoryStream(bytes);
        var bitmap = new Bitmap(stream);
        return bitmap;
    }
}