using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Fly8Cents.Services;

public static class BiliWbiService
{
    private static readonly HttpClient HttpClient = new();

    private static readonly int[] MixinKeyEncTab =
    {
        46, 47, 18, 2, 53, 8, 23, 32, 15, 50, 10, 31, 58, 3, 45, 35,
        27, 43, 5, 49, 33, 9, 42, 19, 29, 28, 14, 39, 12, 38, 41, 13,
        37, 48, 7, 16, 24, 55, 40, 61, 26, 17, 0, 1, 60, 51, 30, 4,
        22, 25, 54, 21, 56, 59, 6, 63, 57, 62, 11, 36, 20, 34, 44, 52
    };

    private static string GetMixinKey(string orig)
    {
        return MixinKeyEncTab.Aggregate("", (s, i) => s + orig[i])[..32];
    }

    private static Dictionary<string, string> EncWbi(Dictionary<string, string> parameters,
        string imgKey,
        string subKey)
    {
        var mixinKey = GetMixinKey(imgKey + subKey);
        var currTime = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

        parameters["wts"] = currTime;
        parameters = parameters.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
        parameters = parameters.ToDictionary(
            kvp => kvp.Key,
            kvp => new string(kvp.Value.Where(chr => !"!'()*".Contains(chr)).ToArray())
        );

        var query = new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result;
        var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(query + mixinKey));
        var wbiSign = Convert.ToHexStringLower(hashBytes);

        parameters["w_rid"] = wbiSign;
        return parameters;
    }

    private static async Task<(string imgKey, string subKey)> GetWbiKeysAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.bilibili.com/x/web-interface/nav");
        request.Headers.Add("User-Agent", "Mozilla/5.0");
        request.Headers.Referrer = new Uri("https://www.bilibili.com/");

        var response = await HttpClient.SendAsync(request);
        var json = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        var imgUrl = (string)json["data"]!["wbi_img"]!["img_url"]!;
        var subUrl = (string)json["data"]!["wbi_img"]!["sub_url"]!;

        var imgKey = imgUrl.Split("/")[^1].Split(".")[0];
        var subKey = subUrl.Split("/")[^1].Split(".")[0];

        return (imgKey, subKey);
    }

    /// <summary>
    ///     给请求参数添加 WBI 签名
    /// </summary>
    public static async Task<Dictionary<string, string>> SignAsync(Dictionary<string, string> parameters)
    {
        var (imgKey, subKey) = await GetWbiKeysAsync();
        return EncWbi(parameters, imgKey, subKey);
    }
}