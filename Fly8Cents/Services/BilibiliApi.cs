using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Fly8Cents.Models;

namespace Fly8Cents.Services;

public class BilibiliApi
{
    private readonly HttpClient _httpClient = new();

    /// <summary>
    ///     获取评论区明细（支持翻页）
    /// </summary>
    /// <param name="oid">评论区目标 ID（视频- avId/cid 等）</param>
    /// <param name="type">评论区类型代码（1=视频，12=专栏，17=动态，等等）</param>
    /// <param name="page">页码（默认 1）</param>
    /// <param name="pageSize">每页数量（1-20，默认 20）</param>
    /// <param name="sort">排序方式（0=时间，1=点赞，2=回复数）</param>
    /// <param name="noHot">是否不显示热评（0=显示，1=不显示）</param>
    public async Task<List<Comment>> GetCommentsAsync(
        long oid,
        CommentType type = CommentType.Video,
        int page = 1,
        int pageSize = 20,
        int sort = 0,
        bool noHot = false)
    {
        var url = new StringBuilder().Append("https://api.bilibili.com/x/v2/reply")
            .Append("?type=")
            .Append((int)type)
            .Append("&oid=")
            .Append(oid)
            .Append("&sort=")
            .Append(sort)
            .Append("&ps=")
            .Append(pageSize)
            .Append("&pn=")
            .Append(page)
            .Append("&nohot=")
            .Append(Convert.ToInt32(noHot))
            .ToString();

        var json = await _httpClient.GetStringAsync(url);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var list = new List<Comment>();
        if (!root.TryGetProperty("data", out var data))
        {
            return list;
        }

        var replies = data.GetProperty("replies");
        list.AddRange(from reply in replies.EnumerateArray()
            let uname = reply.GetProperty("member").GetProperty("uname").GetString() ?? ""
            let message = reply.GetProperty("content").GetProperty("message").GetString() ?? ""
            let ctime = reply.GetProperty("ctime").GetInt64()
            select new Comment { UserName = uname, Message = message, Ctime = ctime });

        return list;
    }
}