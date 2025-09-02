using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;
using QuickType.QrResponse;
using QuickType.QrResponse.QrRequest;
using ReactiveUI;

// 假设 QrRequestData 在这里

namespace Fly8Cents.ViewModels;

public class QrLoginViewModel : ViewModelBase
{
    private readonly HttpClient _httpClient = new();
    private string _helpInfo = "此登录仅用于获取评论等公开信息，请勿滥用账号或爬取敏感数据";

    private string _qrCodePath = "";

    public QrLoginViewModel(MainWindowViewModel mainWindowViewModel)
    {
        RefreshQrCodeCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                var requestResponse = await _httpClient.GetStringAsync(
                    "https://passport.bilibili.com/x/passport-login/web/qrcode/generate"
                );

                var qrRequestData = QrRequestData.FromJson(requestResponse);
                if (qrRequestData.Data.Url != null)
                {
                    await _httpClient.GetStringAsync(
                        "https://passport.bilibili.com/x/passport-login/web/qrcode/poll"
                    );

                    Console.WriteLine($"Url: {qrRequestData.Data.Url}");
                    QrCodePath = qrRequestData.Data.Url.ToString();
                    var dataQrcodeKey = qrRequestData.Data.QrcodeKey;
                    Console.WriteLine($"QrcodeKey: {dataQrcodeKey}");

                    var isBreak = false;
                    while (!isBreak)
                    {
                        var generateResponse = await _httpClient.GetAsync(
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
                                mainWindowViewModel.SessData = GetSessData(generateResponse);
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

    private static string GetSessData(HttpResponseMessage cookieResponse)
    {
        if (!cookieResponse.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            throw new InvalidOperationException("Response does not contain any Set-Cookie headers.");
        }

        foreach (var cookie in cookies)
        {
            if (!cookie.StartsWith("SESSDATA="))
            {
                continue;
            }

            var sessData = cookie.Split(';')[0].Split('=')[1];
            Console.WriteLine($"SESSDATA: {sessData}");
            return sessData;
        }

        throw new KeyNotFoundException("SESSDATA cookie was not found in response.");
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
}