namespace Fly8Cents.Models;

public class Comment
{
    /// <summary>
    ///     用户名
    /// </summary>
    public string UserName { get; set; } = "";

    /// <summary>
    ///     评论信息
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    ///     评论时间戳
    /// </summary>
    public long Ctime { get; set; }
}

public enum CommentType
{
    Video = 1,
    Column = 12,
    /// <summary>
    /// 图片动态
    /// </summary>
    Photo = 11,
    /// <summary>
    /// 文本动态
    /// </summary>
    Post = 17
}