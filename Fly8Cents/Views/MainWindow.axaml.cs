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
    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        // 建议在按钮点击后立即禁用按钮，防止重复点击
        if (sender is Button b1)
        {
            b1.IsEnabled = false;
        }

        Console.WriteLine("Starting FFmpeg process...");
        Console.WriteLine($"Working Directory: {Environment.CurrentDirectory}");
        
        const string ffmpegPath = "ffmpeg";
        
        var fontPath = Environment.CurrentDirectory.Replace("\\", "/") + "/SourceHanSansCN-Normal.otf";
        var textFile = Environment.CurrentDirectory.Replace("\\", "/") + "/crawl_text.txt";
        var outputPath = "/home/aik/Temps/star_wars_crawl.mp4"; // 将输出路径也提取为变量

        var arguments = new StringBuilder().Append("-f lavfi -i nullsrc=s=3840x2160 -vf ")
            .Append("drawbox=t=fill,")
            .Append($"drawtext={fontPath}:")
            .Append($"textfile={textFile}:")
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

        try
        {
            await RunFfmpegAsync(ffmpegPath, arguments);
            Console.WriteLine("FFmpeg process completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred during FFmpeg execution: {ex.Message}");
        }
        finally
        {
            // 无论成功还是失败，都重新启用按钮
            if (sender is Button b2)
            {
                b2.IsEnabled = true;
            }
        }
    }

    private static Task<bool> RunFfmpegAsync(string ffmpegPath, string arguments)
    {
        // 使用 TaskCompletionSource 来创建一个可以手动控制完成状态的 Task
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
                CreateNoWindow = true,
            },
            // 2. 启用 Exited 事件，这是 WaitForExitAsync 的基础
            EnableRaisingEvents = true
        };

        // 3. 订阅事件以实时获取输出
        // FFmpeg 的进度信息通常输出到 StandardError
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
                // 标准输出也打印出来，以防有意外信息
                Console.WriteLine($"[STDOUT] {e.Data}");
            }
        };

        // 绑定进程退出时的事件
        process.Exited += (s, e) =>
        {
            // 检查退出代码
            if (process.ExitCode == 0)
            {
                tcs.SetResult(true); // 成功
            }
            else
            {
                // 如果进程以错误代码退出，则将 Task 设置为异常状态
                tcs.SetException(new Exception($"FFmpeg exited with code {process.ExitCode}"));
            }
            
            // 释放资源
            process.Dispose();
        };

        Console.WriteLine("Executing command:");
        Console.WriteLine($"{ffmpegPath} {arguments}");
        
        process.Start();

        // 4. 开始异步读取输出流
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // 返回可以被 await 的 Task
        return tcs.Task;
    }
}