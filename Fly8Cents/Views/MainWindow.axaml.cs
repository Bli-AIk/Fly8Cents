using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Fly8Cents.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // 1. 将方法签名更改为 async void
    private async void Generate_Video(object? sender, RoutedEventArgs e)
    {
        try
        {
            // 建议在按钮点击后立即禁用按钮，防止重复点击
            if (sender is Button b1)
            {
                b1.IsEnabled = false;
            }

            Console.WriteLine("Starting FFmpeg process...");
            Console.WriteLine($"Working Directory: {Environment.CurrentDirectory}");

            const string ffmpegPath = "ffmpeg";

            var arguments = GetVideoArguments("SourceHanSansCN-Normal.otf", "crawl_text.txt",
                "/home/aik/Temps/star_wars_crawl.mp4");

            await RunFfmpegAsync(ffmpegPath, arguments);
            Console.WriteLine("FFmpeg process completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred during FFmpeg execution: {ex.Message}");
        }
        finally
        {
            if (sender is Button b2)
            {
                b2.IsEnabled = true;
            }
        }
    }

    private static string GetVideoArguments(string fontPath, string textFile, string outputPath)
    {
        var fullFontPath = $"{Environment.CurrentDirectory.Replace("\\", "/")}/{fontPath}";
        var fullTextFile = Environment.CurrentDirectory.Replace("\\", "/") + $"/{textFile}";

        var arguments = new StringBuilder().Append("-f lavfi -i nullsrc=s=3840x2160 -vf ")
            .Append("drawbox=t=fill,")
            .Append($"drawtext={fullFontPath}:")
            .Append($"textfile={fullTextFile}:")
            .Append("x=(w-text_w)/2:y=h-100*t:fontsize=150:fontcolor=0xb89801,")
            .Append("drawbox,")
            .Append("perspective=1050:570:2790:570:-1800:H:W+1800:H:sense=destination,")
            .Append("drawbox=0:0:3840:216:t=fill,")
            .Append("drawbox=0:2160-216:3840:216:t=fill ")
            .Append("-preset ultrafast ")
            .Append("-crf 23 ")
            .Append("-t 10 ")
            .Append(outputPath)
            .Append(" -progress pipe:1")
            .ToString();
        return arguments;
    }

    private static Task<bool> RunFfmpegAsync(string ffmpegPath, string arguments)
    {
        var tcs = new TaskCompletionSource<bool>();

        var process = new Process
        {
            StartInfo =
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        process.ErrorDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine(e.Data);
            }
        };

        process.OutputDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine($"[STDOUT] {e.Data}");
            }
        };

        process.Exited += (s, e) =>
        {
            if (process.ExitCode == 0)
            {
                tcs.SetResult(true);
            }
            else
            {
                tcs.SetException(new Exception($"FFmpeg exited with code {process.ExitCode}"));
            }

            process.Dispose();
        };

        Console.WriteLine("Executing command:");
        Console.WriteLine($"{ffmpegPath} {arguments}");

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return tcs.Task;
    }
}