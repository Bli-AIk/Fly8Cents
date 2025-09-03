using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QuickType.LazyComment.Buvid3.UserSpaceDetails;

namespace Fly8Cents.Services;

public static class BiliService
{
    public static async Task<UserSpaceDetailsData?> GetUserSpaceDetailsData(HttpClient httpClient, string uid)
    {
        var wbiService = new BiliWbiService();
        var signedParams = await wbiService.SignAsync(new Dictionary<string, string>
        {
            { "mid", uid }
        });

        var query = await new FormUrlEncodedContent(signedParams).ReadAsStringAsync();

        var requestUri = $"https://api.bilibili.com/x/space/wbi/acc/info?{query}";
        var response = await httpClient.GetStringAsync(
            requestUri
        );
        Console.WriteLine(httpClient.DefaultRequestHeaders);
        Console.WriteLine(requestUri);

        var obj = JObject.Parse(response);

        var code = (int)(obj["code"] ?? throw new InvalidOperationException());

        if (code == -352)
        {
            Console.WriteLine("风控校验失败");
            return null;
        }

        var data = UserSpaceDetailsData.FromJson(response);
        return data;
    }
}