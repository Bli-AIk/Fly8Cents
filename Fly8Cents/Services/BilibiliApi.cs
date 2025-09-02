using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Fly8Cents.Models;

namespace Fly8Cents.Services;

public class BilibiliApi(HttpClient httpClient)
{
    /// <summary>
    ///     获取评论区明细（支持翻页）
    /// </summary>
    /// <param name="oid">评论区目标 ID（视频- avId/cid 等）</param>
    /// <param name="type">评论区类型代码（1=视频，12=专栏，17=动态，等等）</param>
    /// <param name="page">页码（默认 1）</param>
    /// <param name="pageSize">每页数量（1-20，默认 20）</param>
    /// <param name="sort">排序方式（0=时间（疑似不可用），1=点赞，2=回复数）</param>
    /// <param name="noHot">是否不显示热评（0=显示，1=不显示）</param>
    public async Task<List<Comment>> GetCommentsAsync(
        long oid,
        CommentType type,
        int page = 1,
        int pageSize = 20,
        int sort = 1,
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

        var json = await httpClient.GetStringAsync(url);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("data", out var data) || !data.TryGetProperty("replies", out var replies) ||
            replies.ValueKind == JsonValueKind.Null)
        {
            return [];
        }

        return ParseReplies(replies);
    }

    private static List<Comment> ParseReplies(JsonElement jsonElement)
    {
        var comments = new List<Comment>();
        if (jsonElement.ValueKind != JsonValueKind.Array)
        {
            return comments;
        }

        foreach (var reply in jsonElement.EnumerateArray())
        {
            var comment = new Comment
            {
                UserName = reply.GetProperty("member").GetProperty("uname").GetString() ?? "",
                Message = reply.GetProperty("content").GetProperty("message").GetString() ?? "",
                Ctime = reply.GetProperty("ctime").GetInt64()
            };

            if (reply.TryGetProperty("replies", out var nestedReplies) &&
                nestedReplies.ValueKind == JsonValueKind.Array)
            {
                comment.Replies = ParseReplies(nestedReplies);
            }

            comments.Add(comment);
        }

        return comments;
    }
}