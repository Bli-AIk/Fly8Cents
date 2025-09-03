using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;
using QuickType.Buvid3;
using QuickType.QrResponse;
using QuickType.QrRequest;
using ReactiveUI;

// 假设 QrRequestData 在这里

namespace Fly8Cents.ViewModels;

public class QrLoginViewModel : ViewModelBase
{
    private string _helpInfo = "此登录仅用于获取评论等公开信息，请勿滥用账号或爬取敏感数据。\n登录操作是可选的，但如果不登录，部分公开信息可能无法获取。";

    private string _qrCodePath = "";

    public QrLoginViewModel(HttpClient httpClient)
    {
        RefreshQrCodeCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                var requestResponse = await httpClient.GetStringAsync(
                    "https://passport.bilibili.com/x/passport-login/web/qrcode/generate"
                );

                var qrRequestData = QrRequestData.FromJson(requestResponse);
                if (qrRequestData.Data.Url != null)
                {
                    QrCodePath = qrRequestData.Data.Url.ToString();
                    var dataQrcodeKey = qrRequestData.Data.QrcodeKey;

                    Console.WriteLine($"Url: {QrCodePath}");
                    Console.WriteLine($"QrcodeKey: {dataQrcodeKey}");

                    var isBreak = false;
                    while (!isBreak)
                    {
                        var generateResponse = await httpClient.GetAsync(
                            $"https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key={dataQrcodeKey}"
                        );

                        generateResponse.EnsureSuccessStatusCode();

                        var json = await generateResponse.Content.ReadAsStringAsync();
                        var qrResponseData = QrResponseData.FromJson(json);
                        switch (qrResponseData.Data.Code)
                        {
                            // 未扫码
                            case 86101:
                                await Task.Delay(3000);
                                continue;
                            // 二维码已扫码未确认
                            case 86090:
                                await Task.Delay(3000);
                                HelpInfo = "扫描成功，等待手机端确认……";
                                continue;
                            case 0:
                                HelpInfo = "已登录。";

                                var cookieContainer = new CookieContainer();

                                var baseUri = new Uri("https://www.bilibili.com");
                                cookieContainer.Add(baseUri,
                                    new Cookie("DedeUserID", GetCookieFromQr(generateResponse, "DedeUserID")));
                                cookieContainer.Add(baseUri,
                                    new Cookie("DedeUserID__ckMd5",
                                        GetCookieFromQr(generateResponse, "DedeUserID__ckMd5")));
                                cookieContainer.Add(baseUri,
                                    new Cookie("SESSDATA", GetCookieFromQr(generateResponse, "SESSDATA")));
                                cookieContainer.Add(baseUri,
                                    new Cookie("bili_jct", GetCookieFromQr(generateResponse, "bili_jct")));
                                cookieContainer.Add(baseUri, new Cookie("buvid3", await GetBuvid3(httpClient)));

                                var handler = new HttpClientHandler
                                {
                                    CookieContainer = cookieContainer
                                };
                                httpClient = MainWindowViewModel.GetHttpClient(handler);

                                break;
                            default:
                                HelpInfo = "扫描失败，请重试。";
                                break;
                        }

                        isBreak = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取二维码失败: {ex.Message}");
            }
        });
    }

    public string HelpInfo
    {
        get => _helpInfo;
        set => this.RaiseAndSetIfChanged(ref _helpInfo, value);
    }

    public string QrCodePath
    {
        get => _qrCodePath;
        set => this.RaiseAndSetIfChanged(ref _qrCodePath, value);
    }

    public ReactiveCommand<Unit, Unit> RefreshQrCodeCommand { get; }

    private static async Task<string> GetBuvid3(HttpClient httpClient)
    {
        var response = await httpClient.GetStringAsync(
            "https://api.bilibili.com/x/web-frontend/getbuvid"
        );

        var buvid3Data = Buvid3Data.FromJson(response);
        var buvid3 = buvid3Data.Data.Buvid;
        Console.WriteLine($"buvid3: {buvid3}");
        return buvid3;
    }

    private static string GetCookieFromQr(HttpResponseMessage cookieResponse, string name)
    {
        if (!cookieResponse.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            throw new InvalidOperationException("Response does not contain any Set-Cookie headers.");
        }

        foreach (var cookie in cookies)
        {
            if (!cookie.StartsWith($"{name}="))
            {
                continue;
            }

            var value = cookie.Split(';')[0].Split('=')[1];
            Console.WriteLine($"{name}: {value}");
            return value;
        }

        throw new KeyNotFoundException($"{name} cookie was not found in response.");
    }
}