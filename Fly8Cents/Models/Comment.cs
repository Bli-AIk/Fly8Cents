using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Fly8Cents.Models;

public class Comment
{
    /// <summary>
    ///     用户名
    /// </summary>
    [JsonPropertyName("uname")]
    public string UserName { get; init; } = "";

    /// <summary>
    ///     评论信息
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; init; } = "";

    /// <summary>
    ///     评论时间戳
    /// </summary>
    [JsonPropertyName("ctime")]
    public long Ctime { get; init; }


    [JsonPropertyName("replies")] public List<Comment>? Replies { get; set; }
}

public enum CommentType
{
    Video = 1,
    Column = 12,

    /// <summary>
    ///     图片动态
    /// </summary>
    Photo = 11,

    /// <summary>
    ///     文本动态
    /// </summary>
    Post = 17
}