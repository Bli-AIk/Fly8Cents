using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Fly8Cents.Services;
using ReactiveUI;

namespace Fly8Cents.ViewModels;

public class ExportViewModel : ReactiveObject
{
    private string _commentsText = "";
    private string _consoleOutput = "";
    private int _selectedFrameRate;
    private string _selectedPreset = "";

    // 视频参数
    private string _selectedResolution = "";
    private int _videoDuration = 60;

    public ExportViewModel()
    {
        // 模拟爬取评论
        FetchCommentsCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            ConsoleOutput = "开始爬取评论...\n";
            var sb = new StringBuilder();

            for (var i = 1; i <= 5; i++)
            {
                await Task.Delay(500); // 模拟网络延迟
                var comment = $"评论 {i} 内容";
                sb.AppendLine(comment);
                ConsoleOutput += $"爬取到: {comment}\n";
            }

            CommentsText = sb.ToString();
            ConsoleOutput += "爬取完成。\n";
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
                    "/home/aik/Temps/star_wars_crawl.mp4");

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

    // 基本属性
    public string ConsoleOutput
    {
        get => _consoleOutput;
        set => this.RaiseAndSetIfChanged(ref _consoleOutput, value);
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