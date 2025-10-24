using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuickType.LazyComment;
using QuickType.UserSpaceData;
using QuickType.UserSpaceDetails;
using QuickType.VideoKeywordQuery;

namespace Fly8Cents.Services;

public static class BiliService
{
    /// <summary>
    ///     评论区类型代码（来源不完全可靠，仅供参考）
    /// </summary>
    public enum CommentAreaType
    {
        /// <summary>视频稿件，oid = avid</summary>
        Video = 1,

        /// <summary>话题，oid = 话题 id</summary>
        Topic = 2,

        /// <summary>活动，oid = 活动 id</summary>
        Activity = 4,

        /// <summary>小视频，oid = 小视频 id</summary>
        MiniVideo = 5,

        /// <summary>小黑屋封禁信息，oid = 封禁公示 id</summary>
        BlackroomBanInfo = 6,

        /// <summary>公告信息，oid = 公告 id</summary>
        Announcement = 7,

        /// <summary>直播活动，oid = 直播间 id</summary>
        Live = 8,

        /// <summary>活动稿件，oid = ?</summary>
        ActivityVideo = 9,

        /// <summary>直播公告，oid = ?</summary>
        LiveAnnouncement = 10,

        /// <summary>相簿（图片动态），oid = 相簿 id</summary>
        Album = 11,

        /// <summary>专栏，oid = cvid</summary>
        Article = 12,

        /// <summary>票务，oid = ?</summary>
        Ticket = 13,

        /// <summary>音频，oid = auid</summary>
        Audio = 14,

        /// <summary>风纪委员会，oid = 众裁项目 id</summary>
        Jury = 15,

        /// <summary>点评，oid = ?</summary>
        Review = 16,

        /// <summary>动态（纯文字动态 & 分享），oid = 动态 id</summary>
        Dynamic = 17,

        /// <summary>播单，oid = ?</summary>
        Playlist = 18,

        /// <summary>音乐播单，oid = ?</summary>
        MusicPlaylist = 19,

        /// <summary>漫画，oid = ?</summary>
        Comic = 20,

        /// <summary>漫画，oid = ?</summary>
        ComicAlt = 21,

        /// <summary>漫画，oid = mcid</summary>
        Manga = 22,

        /// <summary>课程，oid = epid</summary>
        Course = 33
    }

    /// <summary>
    ///     根据关键词查找视频。
    /// </summary>
    /// <param name="httpClient">HttpClient 实例</param>
    /// <param name="mid">用户 UID</param>
    /// <param name="keywords">关键词。可为空, 即获取所有视频</param>
    /// <returns>VideoKeywordQueryData</returns>
    /// <remarks>
    ///     参考：
    ///     <see href="https://socialsisteryi.github.io/bilibili-API-collect/docs/video/collection.html" />
    /// </remarks>
    public static async Task<VideoKeywordQueryData> GetVideoKeywordQuery(HttpClient httpClient,
        long mid,
        string keywords = "")
    {
        const string baseUrl = "https://api.bilibili.com/x/series/recArchivesByKeywords";

        var query = new Dictionary<string, string>
        {
            { "mid", mid.ToString() },
            { "keywords", keywords },
            { "ps", "0" }
        };

        var queryString = string.Join("&", query.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        var requestUri = $"{baseUrl}?{queryString}";

        Console.WriteLine("RequestUri: " + requestUri);
        var response = await httpClient.GetStringAsync(requestUri);
        return VideoKeywordQueryData.FromJson(response);
    }

    /// <summary>
    ///     获取用户空间动态。
    /// </summary>
    /// <param name="httpClient">HttpClient 实例</param>
    /// <param name="mid">用户 UID</param>
    /// <param name="offset">分页偏移量</param>
    /// <returns>UserSpaceData</returns>
    /// <remarks>
    ///     参考：
    ///     <see href="https://socialsisteryi.github.io/bilibili-API-collect/docs/dynamic/space.html" />
    /// </remarks>
    public static async Task<UserSpaceData> GetUserSpace(HttpClient httpClient,
        long mid,
        long? offset)
    {
        const string baseUrl = "https://api.bilibili.com/x/polymer/web-dynamic/v1/feed/space";

        var query = new Dictionary<string, string>
        {
            { "host_mid", mid.ToString() }
        };

        if (offset.HasValue)
        {
            query.Add("offset", offset.Value.ToString());
        }

        var queryString = string.Join("&", query.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        var requestUri = $"{baseUrl}?{queryString}";

        Console.WriteLine("RequestUri: " + requestUri);
        var response = await httpClient.GetStringAsync(requestUri);
        return UserSpaceData.FromJson(response);
    }

    /// <summary>
    ///     获取评论区明细（懒加载）。
    /// </summary>
    /// <param name="httpClient">HttpClient 实例</param>
    /// <param name="type">评论区类型代码</param>
    /// <param name="oid">目标评论区 id</param>
    /// <param name="nextOffset">上次响应 data.cursor.nextOffset 的值。为空时获取第一页</param>
    /// <param name="mode">排序方式，详见参考</param>
    /// <returns>LazyCommentData</returns>
    /// <remarks>
    ///     参考：
    ///     <see href="https://socialsisteryi.github.io/bilibili-API-collect/docs/comment/list.html" />
    /// </remarks>
    public static async Task<LazyCommentData> GetLazyComment(HttpClient httpClient,
        CommentAreaType type,
        long oid,
        string nextOffset,
        int mode = 3)
    {
        var parameters = new Dictionary<string, string>
        {
            { "oid", oid.ToString() },
            { "type", ((int)type).ToString() },
            { "mode", mode.ToString() },
            { "web_location", "1315875" },
            { "plat", "1" }
        };

        if (string.IsNullOrEmpty(nextOffset))
        {
            parameters.Add("seek_rpid", "");
        }

        var paginationObject = new { offset = nextOffset };
        var paginationJson = JsonConvert.SerializeObject(paginationObject);
        parameters.Add("pagination_str", paginationJson);

        var signedParams = await BiliWbiService.SignAsync(parameters);

        var query = await new FormUrlEncodedContent(signedParams).ReadAsStringAsync();
        //输出示例： bar=514&baz=1919810&foo=114&wts=1687541921&w_rid=26e82b1b9b3a11dbb1807a9228a40d3b

        var requestUri = $"https://api.bilibili.com/x/v2/reply/wbi/main?{query}";

        Console.WriteLine(httpClient.DefaultRequestHeaders);
        Console.WriteLine("RequestUri: " + requestUri);

        var response = await httpClient.GetStringAsync(requestUri);

        var data = LazyCommentData.FromJson(response);
        return data;
    }

    /// <summary>
    ///     用户空间详细信息
    /// </summary>
    /// <param name="httpClient">HttpClient 实例</param>
    /// <param name="mid">目标用户mid</param>
    /// <returns>UserSpaceDetailsData</returns>
    /// <exception cref="InvalidOperationException">风控校验失败</exception>
    /// <remarks>
    ///     参考：
    ///     <see href="https://socialsisteryi.github.io/bilibili-API-collect/docs/user/info.html" />
    /// </remarks>
    public static async Task<UserSpaceDetailsData?> GetUserSpaceDetailsData(HttpClient httpClient, long mid)
    {
        var signedParams = await BiliWbiService.SignAsync(new Dictionary<string, string>
        {
            { "mid", mid.ToString() }
        });

        var query = await new FormUrlEncodedContent(signedParams).ReadAsStringAsync();
        //输出示例： bar=514&baz=1919810&foo=114&wts=1687541921&w_rid=26e82b1b9b3a11dbb1807a9228a40d3b

        var requestUri = $"https://api.bilibili.com/x/space/wbi/acc/info?{query}";

        Console.WriteLine(httpClient.DefaultRequestHeaders);
        Console.WriteLine("RequestUri: " + requestUri);

        var response = await httpClient.GetStringAsync(requestUri);

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