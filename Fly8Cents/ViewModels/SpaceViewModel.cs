using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;
using Fly8Cents.Services;
using ReactiveUI;

namespace Fly8Cents.ViewModels;

public class SpaceViewModel : ViewModelBase
{
    public SpaceViewModel(HttpClient httpClient)
    {
        ExtractSpaceCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                await FetchSpacesComments(httpClient, long.Parse(InputText));
            }
            catch (Exception ex)
            {
                var format = $"爬取评论时出错: {ex.Message}\n";
                Console.WriteLine(format);
            }
        });
    }

    public ReactiveCommand<Unit, Unit> ExtractSpaceCommand { get; }

    public string InputText
    {
        get => _inputText;
        set => this.RaiseAndSetIfChanged(ref _inputText, value);
    }

    private string _inputText = "";

    public string FinalText
    {
        get => _finalText;
        set => this.RaiseAndSetIfChanged(ref _finalText, value);
    }

    private string _finalText = "";


    private async Task FetchSpacesComments(HttpClient httpClient, long uid)
    {
        ConsoleOutput += "爬取主页动态信息中……\n";
        long? offset = null;
        var isBreak = false;

        while (!isBreak)
        {
            var userSpaceData = await BiliService.GetUserSpace(httpClient, uid, offset);
            if (userSpaceData.Code != 0)
            {
                ConsoleOutput += $"Code: {userSpaceData.Code} 爬取动态失败！\n";
                return;
            }

            ConsoleOutput += $"Code: {userSpaceData.Code} 爬取动态成功。\n";

            if (long.TryParse(userSpaceData.Data.Offset, out var newOffset))
            {
                offset = newOffset;
            }
            else
            {
                isBreak = true;
            }

            foreach (var item in userSpaceData.Data.Items)
            {
                var itemTime = DateTimeOffset.FromUnixTimeSeconds(item.Modules.ModuleAuthor.PubTs);

                var basicCommentType = (BiliService.CommentAreaType)item.Basic.CommentType;

                if (basicCommentType is not (BiliService.CommentAreaType.Album or BiliService.CommentAreaType.Dynamic))
                {
                    continue;
                }

                if (!long.TryParse(item.Basic.CommentIdStr, out var id))
                {
                    continue;
                }

                var itemTimeText = itemTime.ToString("yyyy年MM月dd日 HH:mm:ss", CultureInfo.GetCultureInfo("zh-CN"));

                await OutputComment(httpClient,
                    basicCommentType, id,
                    $"正在处理动态：{id}；发布时间：{itemTimeText}\n",
                    $"{id} 爬取成功。\n",
                    $"{id} 爬取失败。\n");
            }

            await Task.Delay(5000);
        }

        ConsoleOutput += "动态爬取完成。\n";
    }

    private async Task OutputComment(HttpClient httpClient,
        BiliService.CommentAreaType commentAreaType,
        long oid,
        string startConsole,
        string successConsole,
        string errorConsole)
    {
        ConsoleOutput += startConsole;

        var isBreak = false;
        var nextOffset = "";
        while (!isBreak)
        {
            var lazyComment =
                await BiliService.GetLazyComment(httpClient, commentAreaType, oid, nextOffset);
            if (lazyComment.Code != 0)
            {
                ConsoleOutput += errorConsole;
                continue;
            }

            ConsoleOutput += successConsole;

            foreach (var reply in lazyComment.Data.Replies)
            {
                if (reply == null)
                {
                    continue;
                }

                var dateTime = DateTimeOffset.FromUnixTimeSeconds(reply.Ctime);

                var message = reply.Content.Message;
                const string configText = "{year}-{month}-{day} {hour}:{minute}\nid: {uname}\n{message}\n\n";

                var output =
                    StringHelper.FormatComment(dateTime,
                        reply.Member.Uname,
                        reply.Member.Mid,
                        reply.Member.Sign,
                        reply.Member.Level,
                        reply.Member.Sex,
                        reply.Like,
                        message, configText);

                FinalText += output;
                await Task.Delay(500);
            }

            if (lazyComment.Data.Cursor.IsEnd)
            {
                isBreak = true;
            }
            else
            {
                nextOffset = lazyComment.Data.Cursor.PaginationReply.NextOffset;
            }

            if (isBreak)
            {
                continue;
            }

            ConsoleOutput += "爬取完毕，请稍候……\n";
            await Task.Delay(5000);
            ConsoleOutput += $"正在爬取页{lazyComment.Data.Cursor.Next}……\n";
        }
    }

    public string ConsoleOutput
    {
        get => _consoleOutput;
        set
        {
            var oldLines = _consoleOutput.Split('\n');
            var newLines = value.Split('\n');

            // 找出新增的部分
            var addedLines = newLines.Skip(oldLines.Length).ToArray();

            // 打印新增的行
            foreach (var line in addedLines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    ConsoleWriteLine(line);
                }
            }

            // 保留最多10行
            if (newLines.Length > 10)
            {
                newLines = newLines.Skip(newLines.Length - 10).ToArray();
            }

            var newText = string.Join("\n", newLines);
            this.RaiseAndSetIfChanged(ref _consoleOutput, newText);
        }
    }

    private string _consoleOutput = "";

    private void ConsoleWriteLine(string format)
    {
        Console.WriteLine(format);
        ConsoleOutput += $"{format}\n";
    }
}