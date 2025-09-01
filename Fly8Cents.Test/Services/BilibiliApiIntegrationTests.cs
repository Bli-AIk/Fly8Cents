using Fly8Cents.Models;
using Fly8Cents.Services;
using Xunit.Abstractions;

namespace Fly8Cents.Test.Services;

public class BilibiliApiIntegrationTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    public BilibiliApiIntegrationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    // 在 Xunit 中，你可以使用 Trait 特性来给测试分类
    // 这样在测试运行器中就可以选择只跑单元测试或只跑集成测试
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCommentsAsync_ShouldFetchRealComments_FromLiveApi()
    {
        // --- Arrange ---

        // 1. 准备一个真实的 HttpClient。
        // 在真实应用中，推荐使用 IHttpClientFactory 来管理 HttpClient 实例。
        var httpClient = new HttpClient();
        
        // 添加 User-Agent 头，模拟浏览器访问，有些 API 如果没有这个头会拒绝访问
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36");

        var bilibiliApi = new BilibiliApi(httpClient);
        
        // 2. 定义一个真实的、不太可能被删除的视频 OID
        long videoOid = 2; // 《最终鬼畜蓝蓝路》

        // --- Act ---

        // 3. 调用方法，这次会发起一个真实的网络请求
        var comments = await bilibiliApi.GetCommentsAsync(
            oid: videoOid,
            type: CommentType.Video
        );

        // --- Assert ---

        _testOutputHelper.WriteLine(comments.ToString());
        _testOutputHelper.WriteLine(comments.Count.ToString());
        // 4. 对真实返回的数据进行断言
        // 因为我们无法预测评论的具体内容，所以我们只检查数据的“合理性”
        Assert.NotNull(comments);
        Assert.NotEmpty(comments); // 对于这个热门视频，评论列表不应为空
        Assert.Equal(20, comments.Count); // 检查是否返回了我们请求的 pageSize 数量的评论

        // 遍历返回的评论，检查关键字段是否有效
        foreach (var comment in comments)
        {
            Assert.False(string.IsNullOrWhiteSpace(comment.UserName)); // 用户名不应为空
            Assert.NotNull(comment.Message); // 评论消息不应为 null（可能为空字符串）
            Assert.True(comment.Ctime > 0); // 评论时间戳应该是有效的正数
        }
    }
}