using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive;
using Fly8Cents.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuickType.Buvid3.UserSpaceDetails;
using ReactiveUI;

namespace Fly8Cents.ViewModels;

public class BasicInfoViewModel : ViewModelBase
{
    private DateTimeOffset _endDate = DateTimeOffset.Now;

    private DateTimeOffset _startDate = DateTimeOffset.Now.AddDays(-7);
    private string _uid = "1424609716";
    
    private string _uploaderAvatarUrl = "";

    private string _uploaderNickname = "";

    public BasicInfoViewModel(HttpClient httpClient)
    {
        CheckUid = ReactiveCommand.CreateFromTask(async () =>
        {
            var wbiService = new BiliWbiService();
            var signedParams = await wbiService.SignAsync(new Dictionary<string, string>
            {
                { "mid", _uid }
            });

            var query = await new FormUrlEncodedContent(signedParams).ReadAsStringAsync();

            var requestUri = $"https://api.bilibili.com/x/space/wbi/acc/info?{query}";
            var response = await httpClient.GetStringAsync(
                requestUri
                );
            Console.WriteLine(httpClient.DefaultRequestHeaders);
            Console.WriteLine(requestUri);
            Console.WriteLine("RES\n\n"+response);
            
            var obj = JObject.Parse(response);

            var code = (int)(obj["code"] ?? throw new InvalidOperationException());

            if (code == -352)
            {
                Console.WriteLine("风控校验失败");
                return;
            }

            var data = UserSpaceDetailsData.FromJson(response);
            UploaderNickname = data.Data.Name;
            UploaderAvatarUrl = data.Data.Face.ToString();
        });
    }

    public string Uid
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