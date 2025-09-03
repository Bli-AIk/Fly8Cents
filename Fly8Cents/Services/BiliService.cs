using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QuickType.LazyComment;
using QuickType.UserSpaceDetails;

namespace Fly8Cents.Services;

public static class BiliService
{
    /// <summary>
    /// 评论区类型代码（来源不完全可靠，仅供参考）
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
        Course = 33,
    }

    public static async Task<LazyCommentData> GetLazyComment(HttpClient httpClient,
        CommentAreaType type,
        long oid,
        int mode = 3) 
    {
        var wbiService = new BiliWbiService();
        var signedParams = await wbiService.SignAsync(new Dictionary<string, string>
        {
            { "oid", oid.ToString() },
            { "type", ((int)type).ToString() },
            { "mode", mode.ToString() }
        });

        var query = await new FormUrlEncodedContent(signedParams).ReadAsStringAsync();
        //输出示例： bar=514&baz=1919810&foo=114&wts=1687541921&w_rid=26e82b1b9b3a11dbb1807a9228a40d3b

        var requestUri = $"https://api.bilibili.com/x/v2/reply/wbi/main?{query}";
        
        Console.WriteLine(httpClient.DefaultRequestHeaders);
        Console.WriteLine(requestUri);
        
        var response = await httpClient.GetStringAsync(
            requestUri
        );
        
        var data = LazyCommentData.FromJson(response);
        return data;
    }
    
    public static async Task<UserSpaceDetailsData?> GetUserSpaceDetailsData(HttpClient httpClient, long uid)
    {
        var wbiService = new BiliWbiService();
        var signedParams = await wbiService.SignAsync(new Dictionary<string, string>
        {
            { "mid", uid.ToString() }
        });

        var query = await new FormUrlEncodedContent(signedParams).ReadAsStringAsync();
        //输出示例： bar=514&baz=1919810&foo=114&wts=1687541921&w_rid=26e82b1b9b3a11dbb1807a9228a40d3b

        var requestUri = $"https://api.bilibili.com/x/space/wbi/acc/info?{query}";
        
        Console.WriteLine(httpClient.DefaultRequestHeaders);
        Console.WriteLine(requestUri);
        
        var response = await httpClient.GetStringAsync(
            requestUri
        );

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