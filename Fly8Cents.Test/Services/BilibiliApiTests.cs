using System.Net;
using Fly8Cents.Models;
using Fly8Cents.Services;
using JetBrains.Annotations;
using Moq;
using Moq.Protected;

namespace Fly8Cents.Test.Services;

[TestSubject(typeof(BilibiliApi))]
public class BilibiliApiTests
{
    // 测试场景1：当 API 返回成功且包含有效评论数据时，应能正确解析并返回评论列表
    [Fact]
    public async Task GetCommentsAsync_ShouldReturnParsedComments_WhenApiReturnsValidData()
    {
        // --- Arrange ---

        // 1. 准备一个模拟的 API 成功响应 JSON
        var fakeJsonResponse = @"
        {
            ""code"": 0,
            ""message"": ""0"",
            ""ttl"": 1,
            ""data"": {
                ""replies"": [
                    {
                        ""rpid"": 12345,
                        ""oid"": 98765,
                        ""type"": 1,
                        ""mid"": 54321,
                        ""root"": 0,
                        ""parent"": 0,
                        ""dialog"": 0,
                        ""count"": 10,
                        ""rcount"": 0,
                        ""state"": 0,
                        ""fansgrade"": 0,
                        ""attr"": 0,
                        ""ctime"": 1672531200,
                        ""rpid_str"": ""12345"",
                        ""like"": 100,
                        ""action"": 0,
                        ""member"": {
                            ""mid"": ""54321"",
                            ""uname"": ""TestUser1"",
                            ""sex"": ""男"",
                            ""sign"": ""签名""
                        },
                        ""content"": {
                            ""message"": ""这是一个测试评论！"",
                            ""plat"": 1
                        }
                    }
                ]
            }
        }";

        // 2. 设置 Moq 来模拟 HttpMessageHandler
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            // 拦截任何发送到 HttpClient 的请求
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            // 返回一个包含我们伪造的 JSON 的成功响应
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fakeJsonResponse)
            })
            .Verifiable(); // 标记这个设置是可验证的

        // 3. 创建一个使用模拟处理程序的 HttpClient 实例
        var httpClient = new HttpClient(handlerMock.Object);

        // 4. 实例化被测试的 BilibiliApi
        var bilibiliApi = new BilibiliApi(httpClient);

        // --- Act ---

        // 5. 调用我们要测试的方法
        var comments = await bilibiliApi.GetCommentsAsync(98765, CommentType.Video);

        // --- Assert ---

        // 6. 验证结果是否符合预期
        Assert.NotNull(comments);
        Assert.Single(comments); // 检查列表是否只包含一个评论

        var firstComment = comments[0];
        Assert.Equal("TestUser1", firstComment.UserName);
        Assert.Equal("这是一个测试评论！", firstComment.Message);
        Assert.Equal(1672531200, firstComment.Ctime);

        // 7. 验证预期的 URL 是否被调用 (可选但推荐)
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(1), // 确保请求只被发送了一次
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get
                && req.RequestUri.ToString() ==
                "https://api.bilibili.com/x/v2/reply?type=1&oid=98765&sort=0&ps=20&pn=1&nohot=0"
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    // 测试场景2：当 API 响应中没有 'data' 字段时，应返回一个空列表
    [Fact]
    public async Task GetCommentsAsync_ShouldReturnEmptyList_WhenApiReturnsNoDataField()
    {
        // --- Arrange ---

        // 1. 准备一个不包含 "data" 字段的模拟 JSON
        var fakeJsonResponse = @"
        {
            ""code"": -404,
            ""message"": ""啥都木有"",
            ""ttl"": 1
        }";

        // 2. 同样，设置 Moq
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK, // API 本身可能返回200，但业务码是失败的
                Content = new StringContent(fakeJsonResponse)
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var bilibiliApi = new BilibiliApi(httpClient);

        // --- Act ---
        var comments = await bilibiliApi.GetCommentsAsync(123);

        // --- Assert ---
        Assert.NotNull(comments); // 列表不应为 null
        Assert.Empty(comments); // 列表应为空
    }
}