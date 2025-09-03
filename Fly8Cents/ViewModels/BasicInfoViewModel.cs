using System;
using System.IO;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Fly8Cents.Models;
using Fly8Cents.Services;
using ReactiveUI;

namespace Fly8Cents.ViewModels;

public class BasicInfoViewModel : ViewModelBase
{
    private DateTimeOffset _endDate = DateTimeOffset.Now;

    private DateTimeOffset _startDate = DateTimeOffset.Now.AddDays(-7);

    private UploaderInfoModel _uploader = new()
    {
        Uid = 1424609716,
        Nickname = "请输入文本",
        AvatarUrl = string.Empty
    };

    private Bitmap _uploaderAvatar;

    public BasicInfoViewModel(HttpClient httpClient)
    {
        _uploaderAvatar = GetDefaultBitmap();
        ExtractUid = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                var data = await BiliService.GetUserSpaceDetailsData(httpClient, Uploader.Uid);
                if (data != null)
                {
                    Uploader.Nickname = data.Data.Name;
                    Uploader.AvatarUrl = data.Data.Face.ToString();
                    UploaderAvatar = await SetImageFromUrl(new Uri(Uploader.AvatarUrl));
                }
                else
                {
                    Uploader.Nickname = "风控校验失败，请登录后重试";
                    UploaderAvatar = GetDefaultBitmap();
                }
                MessageBus.Current.SendMessage(Uploader);
            }
            catch (Exception e)
            {
                Console.WriteLine($"ExtractUid Command报错：{e}");
            }
        });
    }

    public UploaderInfoModel Uploader
    {
        get => _uploader;
        set => this.RaiseAndSetIfChanged(ref _uploader, value);
    }
    public DateTimeOffset StartDate
    {
        get => new(_startDate.Year, _startDate.Month, _startDate.Day, 0, 0, 0, _startDate.Offset);
        set
        {
            this.RaiseAndSetIfChanged(ref _startDate, value);
            MessageBus.Current.SendMessage(StartDate, "StartDate");
        }
    }

    public DateTimeOffset EndDate
    {
        get => new(_endDate.Year, _endDate.Month, _endDate.Day, 23, 59, 59, _endDate.Offset);
        set
        {
            this.RaiseAndSetIfChanged(ref _endDate, value); 
            MessageBus.Current.SendMessage(EndDate, "EndDate");
        }
    }

    public ReactiveCommand<Unit, Unit> ExtractUid { get; }


    public Bitmap UploaderAvatar
    {
        get => _uploaderAvatar;
        set => this.RaiseAndSetIfChanged(ref _uploaderAvatar, value);
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