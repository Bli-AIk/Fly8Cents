using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using Fly8Cents.Models;
using Fly8Cents.Services;
using QuickType.VideoKeywordQuery;
using ReactiveUI;

namespace Fly8Cents.ViewModels;

public class ExportViewModel : ReactiveObject
{
    private string _commentsText = "";
    private string _consoleOutput = "";

    private DateTimeOffset _endDate = DateTimeOffset.Now;
    private int _selectedFrameRate;
    private string _selectedPreset = "";

    // 视频参数
    private string _selectedResolution = "";

    private DateTimeOffset _startDate = DateTimeOffset.Now.AddDays(-7);

    private UploaderInfoModel _uploader = new();
    private int _videoDuration = 60;

    public ExportViewModel(HttpClient httpClient)
    {
        MessageBus.Current.Listen<UploaderInfoModel>()
            .Subscribe(uploader => { Uploader = uploader; });
        MessageBus.Current.Listen<DateTimeOffset>("StartDate")
            .Subscribe(startDate => { StartDate = startDate; });
        MessageBus.Current.Listen<DateTimeOffset>("EndDate")
            .Subscribe(endDate => { EndDate = endDate; });

        FetchCommentsCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                CommentsText = "";

                ConsoleOutput = $"准备爬取信息。\n目标UP主: {Uploader.Nickname} (Uid:{Uploader.Uid})\n\n爬取主页视频信息中……\n";

                var videoKeywordQueryData = await BiliService.GetVideoKeywordQuery(httpClient, Uploader.Uid);
                if (videoKeywordQueryData.Code != 0)
                {
                    ConsoleOutput += "爬取视频失败，请重试";
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
                    ConsoleOutput += $"正在处理视频：{item.Bvid}；\n视频标题：{item.Title}\n";

                    var isBreak = false;
                    var nextOffset = "";
                    while (!isBreak)
                    {
                        var lazyComment =
                            await BiliService.GetLazyComment(httpClient, BiliService.CommentAreaType.Video, item.Aid,
                                nextOffset);
                        if (lazyComment.Code != 0)
                        {
                            ConsoleOutput += $"{item.Bvid} 获取失败。\n";
                            continue;
                        }

                        foreach (var reply in lazyComment.Data.Replies)
                        {
                            if (reply == null)
                            {
                                continue;
                            }

                            var dateTime = DateTimeOffset.FromUnixTimeSeconds(reply.Ctime);
                            var dateText = dateTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                            CommentsText += $"{dateText}\nid: {reply.Member.Uname}\n{reply.Content.Message}\n\n";
                        }

                        if (lazyComment.Data.Cursor.IsEnd)
                        {
                            isBreak = true;
                        }
                        else
                        {
                            nextOffset = lazyComment.Data.Cursor.PaginationReply.NextOffset;
                        }
                    }
                }

                ConsoleOutput += "爬取完成。\n";
            }
            catch (Exception ex)
            {
                var format = $"爬取评论时出错: {ex.Message}\n";
                Console.WriteLine(format);
                ConsoleOutput += format;
            }
        });

        // 导出命令（具体实现留空）
        ExportCommentsCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                Console.WriteLine("Starting FFmpeg process...");
                Console.WriteLine($"Working Directory: {Environment.CurrentDirectory}");

                const string ffmpegPath = "ffmpeg";

                var arguments = VideoGenerateService.GetVideoArguments("SourceHanSansCN-Normal.otf", "crawl_text.txt",
                    "/home/aik/Temps/star_wars_crawl.mp4", "ultrafast");

                await VideoGenerateService.RunFfmpegAsync(ffmpegPath, arguments);
                Console.WriteLine("FFmpeg process completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during FFmpeg execution: {ex.Message}");
            }
        });

        // 初始化视频参数选项
        ResolutionOptions = ["1920x1080", "1280x720", "854x480"];
        FrameRateOptions = [24, 30, 60];
        PresetOptions = ["ultrafast", "superfast", "veryfast", "slow", "veryslow"];

        // 默认值
        SelectedResolution = ResolutionOptions[0];
        SelectedFrameRate = FrameRateOptions[1];
        SelectedPreset = PresetOptions[2];
    }

    public DateTimeOffset StartDate
    {
        get => _startDate;
        set => this.RaiseAndSetIfChanged(ref _startDate, value);
    }

    public DateTimeOffset EndDate
    {
        get => _endDate;
        set => this.RaiseAndSetIfChanged(ref _endDate, value);
    }

    private UploaderInfoModel Uploader
    {
        get => _uploader;
        set => this.RaiseAndSetIfChanged(ref _uploader, value);
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
                    Console.WriteLine(line);
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
    public ReactiveCommand<Unit, Unit> ExportCommentsCommand { get; }
}