using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fly8Cents.Models;
using Fly8Cents.Services;
using QuickType.VideoKeywordQuery;
using ReactiveUI;

namespace Fly8Cents.ViewModels;

public class ExportViewModel : ReactiveObject
{
    private readonly ObservableAsPropertyHelper<string> _estimatedTimeText;
    private string _commentsText = "";
    private ConfigModel _config = new();
    private string _consoleOutput = "";

    private DateTimeOffset _endDate = DateTimeOffset.Now;
    private bool _isExcludeOutOfRangeComments;
    private bool _isFetchSpaces = true;
    private bool _isFetchVideos = true;
    private int _selectedFrameRate;
    private string _selectedPreset = "";

    // 视频参数
    private string _selectedResolution = "";

    private DateTimeOffset _startDate = DateTimeOffset.Now.AddDays(-7);

    private TextSettingsModel _textSettings = new();

    private UploaderInfoModel _uploader = new();
    private int _videoDuration = 60;

    public ExportViewModel(HttpClient httpClient)
    {
        _estimatedTimeText = this.WhenAnyValue(x => x.CommentsText)
            .Select(text =>
            {
                if (string.IsNullOrEmpty(text))
                {
                    return "";
                }

                var lines = VideoGenerateService.WrapString(text).Split([Environment.NewLine], StringSplitOptions.None)
                    .Length;
                return $"格式化后行数：{lines}。预计导出时长为{lines * 2 + 16}秒。";
            })
            .ToProperty(this, x => x.EstimatedTimeText);

        MessageBus.Current.Listen<UploaderInfoModel>()
            .Subscribe(uploader => { Uploader = uploader; });
        MessageBus.Current.Listen<ConfigModel>()
            .Subscribe(config => { Config = config; });
        MessageBus.Current.Listen<DateTimeOffset>("StartDate")
            .Subscribe(startDate => { StartDate = startDate; });
        MessageBus.Current.Listen<DateTimeOffset>("EndDate")
            .Subscribe(endDate => { EndDate = endDate; });
        MessageBus.Current.Listen<TextSettingsModel>()
            .Subscribe(textSettings => { TextSettings = textSettings; });

        FetchCommentsCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                CommentsText = "";

                ConsoleOutput = $"准备爬取信息。\n目标UP主: {Uploader.Nickname} (Uid:{Uploader.Uid})\n\n";

                if (IsFetchVideos)
                {
                    await FetchVideoComments(httpClient);
                }

                if (IsFetchSpaces)
                {
                    await FetchSpacesComments(httpClient);
                }
            }
            catch (Exception ex)
            {
                var format = $"爬取评论时出错: {ex.Message}\n";
                ConsoleWriteLine(format);
            }
        });

        LoadTextCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                var assemblyPath = Assembly.GetExecutingAssembly().Location;
                if (string.IsNullOrEmpty(assemblyPath))
                {
                    throw new InvalidOperationException("无法获取程序集路径。");
                }

                var appDirectory = Path.GetDirectoryName(assemblyPath);
                if (appDirectory == null)
                {
                    throw new InvalidOperationException("无法获取程序目录。");
                }

                var filePath = Path.Combine(appDirectory, "Comments.txt");

                if (File.Exists(filePath))
                {
                    CommentsText = await File.ReadAllTextAsync(filePath);
                    ConsoleWriteLine("文本已加载");
                }
                else
                {
                    ConsoleWriteLine($"未找到 {filePath} 对应的文件。");
                    CommentsText = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ConsoleWriteLine($"加载文件失败：{ex}");
                CommentsText = string.Empty;
            }
        });

        SaveTextCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                var assemblyPath = Assembly.GetExecutingAssembly().Location;
                if (string.IsNullOrEmpty(assemblyPath))
                {
                    throw new InvalidOperationException("无法获取程序集路径。");
                }

                var appDirectory = Path.GetDirectoryName(assemblyPath);
                if (appDirectory == null)
                {
                    throw new InvalidOperationException("无法获取程序目录。");
                }

                var filePath = Path.Combine(appDirectory, "Comments.txt");

                await File.WriteAllTextAsync(filePath, CommentsText);
                ConsoleWriteLine("文本已保存");
            }
            catch (Exception ex)
            {
                ConsoleWriteLine($"保存文件失败：{ex}");
            }
        });


        ExportCommentsCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                if (string.IsNullOrEmpty(CommentsText))
                {
                    ConsoleWriteLine("请在导出前输入文本。");
                    return;
                }

                ConsoleWriteLine("正在格式化文本……");
                var formattedText = VideoGenerateService.WrapString(CommentsText);
                ConsoleWriteLine("文本格式化完毕。");

                ConsoleWriteLine("准备启动FFMPEG进程……");

                const string ffmpegPath = "ffmpeg";

                var lineCount = -1;
                using (var reader = new StringReader(formattedText))
                {
                    while (reader.ReadLine() is { } formattedTextLine)
                    {
                        lineCount++;

                        if (string.IsNullOrWhiteSpace(formattedTextLine))
                        {
                            continue;
                        }

                        var textPngArguments =
                            VideoGenerateService.GetTextPngArguments(SelectedResolution, formattedTextLine, lineCount);
                        await VideoGenerateService.RunFfmpegAsync(ffmpegPath, textPngArguments);

                        ConsoleWriteLine($"第{lineCount}张文本图片导出完毕。");
                    }
                }

                ConsoleWriteLine("所有文本图片导出完毕。\n准备拼接……");

                ImageStitcher.StitchImages(lineCount);
                ConsoleWriteLine("拼接完毕。\n正在清理冗余图片…");
                ImageStitcher.DeleteImages(lineCount);
                ConsoleWriteLine("清理完毕。");

                ConsoleWriteLine("开始导出视频1（片头）。");
                var video1Arguments =
                    VideoGenerateService.GetVideo1Arguments(SelectedResolution, SelectedPreset, SelectedFrameRate,
                        TextSettings.StartText);
                await VideoGenerateService.RunFfmpegAsync(ffmpegPath, video1Arguments);
                ConsoleWriteLine("视频1（片头）导出完毕。");

                ConsoleWriteLine("开始导出视频3（片尾）。");
                var video3Arguments = VideoGenerateService.GetVideo2Arguments(SelectedResolution, SelectedPreset,
                    SelectedFrameRate, VideoDuration);
                await VideoGenerateService.RunFfmpegAsync(ffmpegPath, video3Arguments);
                ConsoleWriteLine("视频3（片尾）导出完毕。");

                ConsoleWriteLine("开始导出视频2（正片）。");
                var video2Arguments =
                    VideoGenerateService.GetVideo3Arguments(SelectedResolution, SelectedPreset, SelectedFrameRate,
                        TextSettings.EndText);
                await VideoGenerateService.RunFfmpegAsync(ffmpegPath, video2Arguments);
                ConsoleWriteLine("视频2（正片）导出完毕。");

                ConsoleWriteLine("所有视频导出完毕。\n正在合并……");
                
                
                var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                if (exeDir == null)
                {
                    throw new InvalidOperationException("无法获取当前程序集所在目录。");
                }

                var output1Path = Path.Combine(exeDir, "output_1.mp4");
                var output2Path = Path.Combine(exeDir, "output_2.mp4");
                var output3Path = Path.Combine(exeDir, "output_3.mp4");
                var outputPath = Path.Combine(exeDir, "output.mp4");
                var outputWithAudioPath = Path.Combine(exeDir, "output_with_audio.mp4");
                var bgmPath = Path.Combine(exeDir, "Assets/Audios/Bgm.mp3");
                await VideoGenerateService.RunFfmpegAsync(ffmpegPath,
                    $"-i \"{output1Path}\" -i \"{output2Path}\" -i \"{output3Path}\" -filter_complex \"[0:v][1:v][2:v]concat=n=3:v=1:a=0[outv]\" -map \"[outv]\" \"{outputPath}\" -y -progress pipe:1");
                ConsoleWriteLine("合并完成。正在生成带有Bgm的副本……");
                await VideoGenerateService.RunFfmpegAsync(ffmpegPath,
                    $"-i \"{outputPath}\" -stream_loop -1 -i \"{bgmPath}\" -c:v copy -c:a aac -map 0:v:0 -map 1:a:0 -shortest \"{outputWithAudioPath}\".mp4 -y -progress pipe:1");
                ConsoleWriteLine("副本已生成。");
                ConsoleWriteLine("导出完毕！请在软件同级目录下获取成片，开始欣赏吧～");
            }
            catch (Exception ex)
            {
                ConsoleWriteLine($"An error occurred during FFmpeg execution: {ex.Message}");
            }
        });

        // 初始化视频参数选项
        ResolutionOptions = ["3840x2160", "1920x1080", "1280x720", "854x480"];
        FrameRateOptions = [24, 30, 60];
        PresetOptions = ["ultrafast", "superfast", "veryfast", "slow", "veryslow"];

        // 默认值
        SelectedResolution = ResolutionOptions[0];
        SelectedFrameRate = FrameRateOptions[2];
        SelectedPreset = PresetOptions[2];
    }

    public TextSettingsModel TextSettings
    {
        get => _textSettings;
        set => this.RaiseAndSetIfChanged(ref _textSettings, value);
    }

    public bool IsExcludeOutOfRangeComments
    {
        get => _isExcludeOutOfRangeComments;
        set => this.RaiseAndSetIfChanged(ref _isExcludeOutOfRangeComments, value);
    }

    public bool IsFetchVideos
    {
        get => _isFetchVideos;
        set => this.RaiseAndSetIfChanged(ref _isFetchVideos, value);
    }

    public bool IsFetchSpaces
    {
        get => _isFetchSpaces;
        set => this.RaiseAndSetIfChanged(ref _isFetchSpaces, value);
    }

    private DateTimeOffset StartDate
    {
        get => _startDate;
        set => this.RaiseAndSetIfChanged(ref _startDate, value);
    }

    private DateTimeOffset EndDate
    {
        get => _endDate;
        set => this.RaiseAndSetIfChanged(ref _endDate, value);
    }

    private UploaderInfoModel Uploader
    {
        get => _uploader;
        set => this.RaiseAndSetIfChanged(ref _uploader, value);
    }

    private ConfigModel Config
    {
        get => _config;
        set => this.RaiseAndSetIfChanged(ref _config, value);
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


    public string CommentsText
    {
        get => _commentsText;
        set => this.RaiseAndSetIfChanged(ref _commentsText, value);
    }

    public string EstimatedTimeText => _estimatedTimeText.Value;

    // 视频参数属性
    public List<string> ResolutionOptions { get; }

    public string SelectedResolution
    {
        get => _selectedResolution;
        set => this.RaiseAndSetIfChanged(ref _selectedResolution, value);
    }

    public List<int> FrameRateOptions { get; }

    public int SelectedFrameRate
    {
        get => _selectedFrameRate;
        set => this.RaiseAndSetIfChanged(ref _selectedFrameRate, value);
    }

    public int VideoDuration
    {
        get => _videoDuration;
        set => this.RaiseAndSetIfChanged(ref _videoDuration, value);
    }

    public List<string> PresetOptions { get; }

    public string SelectedPreset
    {
        get => _selectedPreset;
        set => this.RaiseAndSetIfChanged(ref _selectedPreset, value);
    }

    // Commands
    public ReactiveCommand<Unit, Unit> FetchCommentsCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadTextCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveTextCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportCommentsCommand { get; }

    private async Task FetchVideoComments(HttpClient httpClient)
    {
        ConsoleOutput += "爬取主页视频信息中……\n";
        var videoKeywordQueryData = await BiliService.GetVideoKeywordQuery(httpClient, Uploader.Uid);
        if (videoKeywordQueryData.Code != 0)
        {
            ConsoleOutput += $"Code: {videoKeywordQueryData.Code}\n爬取视频失败！";
            return;
        }

        var fullArchives = videoKeywordQueryData.Data.Archives;

        var startText = StartDate.ToString("yyyy年MM月dd日 HH:mm:ss", CultureInfo.GetCultureInfo("zh-CN"));
        var endText = EndDate.ToString("yyyy年MM月dd日 HH:mm:ss", CultureInfo.GetCultureInfo("zh-CN"));
        ConsoleOutput += $"爬取全部视频成功。共获取到{fullArchives.Length}条视频。\n\n正在计算 {startText} 至 {endText} 发布的视频……";

        var archives = new List<Archive>();
        foreach (var item in fullArchives)
        {
            var videoDate = DateTimeOffset.FromUnixTimeSeconds(item.Pubdate);
            var isInRange = videoDate >= StartDate && videoDate <= EndDate;

            if (!isInRange)
            {
                continue;
            }

            var videoTimeText = videoDate.ToString("yyyy年MM月dd日 HH:mm:ss", CultureInfo.GetCultureInfo("zh-CN"));

            ConsoleOutput += $"添加待爬取视频：{item.Bvid}；\n视频标题：{item.Title}；\n发布时间：{videoTimeText}\n\n";
            archives.Add(item);
        }

        ConsoleOutput += "准备批量检索评论……\n";
        foreach (var item in archives)
        {
            await OutputComment(httpClient,
                BiliService.CommentAreaType.Video, item.Aid,
                $"正在处理视频：{item.Bvid}；\n视频标题：{item.Title}\n",
                $"{item.Bvid} 爬取成功。\n",
                $"{item.Bvid} 爬取失败。\n");
        }

        ConsoleOutput += "视频爬取完成。\n";
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

                if (IsExcludeOutOfRangeComments)
                {
                    var isInRange = dateTime >= StartDate && dateTime <= EndDate;

                    if (!isInRange)
                    {
                        continue;
                    }
                }

                var message = reply.Content.Message;

                var hasBlacklistedWord = Config.BlackList.Any(blacklistedWord =>
                    (!Config.IsBlackHomonym && message.Contains(blacklistedWord)) ||
                    (Config.IsBlackHomonym &&
                     HomophoneChecker.HasHomophone(message,
                         blacklistedWord)));

                var hasWhitelistedWord = Config.WhiteList.Any(whitelistedWord =>
                    (!Config.IsWhiteHomonym && message.Contains(whitelistedWord)) ||
                    (Config.IsWhiteHomonym &&
                     HomophoneChecker.HasHomophone(message,
                         whitelistedWord)));

                if ((Config.BlackList.Count > 0 && hasBlacklistedWord) ||
                    (Config.WhiteList.Count > 0 && !hasWhitelistedWord))
                {
                    continue;
                }

                message = EmoticonHelper.ProcessBilibiliEmoticon(message);

                var dateText = dateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                CommentsText += $"{dateText}\nid: {reply.Member.Uname}\n{message}\n\n";
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

    private async Task FetchSpacesComments(HttpClient httpClient)
    {
        ConsoleOutput += "爬取主页动态信息中……\n";
        long? offset = null;
        var isBreak = false;

        while (!isBreak)
        {
            var userSpaceData = await BiliService.GetUserSpace(httpClient, Uploader.Uid, offset);
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

                var isInRange = itemTime >= StartDate && itemTime <= EndDate;

                if (!isInRange)
                {
                    if (item.Modules.ModuleTag is { Text: "置顶" })
                    {
                        continue;
                    }

                    isBreak = true;
                    break;
                }

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

    private void ConsoleWriteLine(string format)
    {
        Console.WriteLine(format);
        ConsoleOutput += $"{format}\n";
    }
}