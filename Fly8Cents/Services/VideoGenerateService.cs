using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fly8Cents.Services;

public static class VideoGenerateService
{
    public static string GetVideoArguments(string resolution, string preset)
    {
        var fullFontPath = $"{Assembly.GetExecutingAssembly().Location}/SourceHanSansCN-Normal.otf";
        var fullTextFile = $"{Assembly.GetExecutingAssembly().Location}/Comments.txt";
        var outputPath = $"{Assembly.GetExecutingAssembly().Location}/output.mp4";

        // For ffmpeg
        // 标准分辨率为 3840x2160
        var dimensions = resolution.Split('x');

        var width = int.Parse(dimensions[0]);
        var height = int.Parse(dimensions[1]);

        var arguments = new StringBuilder().Append($"-f lavfi -i nullsrc=s={resolution} -vf ")
            .Append("drawbox=t=fill,")
            .Append($"drawtext={fullFontPath}:")
            .Append($"textfile={fullTextFile}:")
            .Append("x=(w-text_w)/2:y=h-100*t:fontsize=150:fontcolor=0xb89801,")
            .Append("drawbox,")
            .Append("perspective=1050:570:2790:570:-1800:H:W+1800:H:sense=destination,")
            .Append($"drawbox=0:0:{width}:{height * 0.1}:t=fill,") // 使用变量
            .Append($"drawbox=0:{height - height * 0.1}:{width}:{height * 0.1}:t=fill ") // 使用变量
            .Append($"-preset {preset} ")
            .Append("-crf 23 ")
            .Append("-t 10 ")
            .Append(outputPath)
            .Append(" -progress pipe:1")
            .ToString();
        return arguments;
    }

    public static Task<bool> RunFfmpegAsync(string ffmpegPath, string arguments)
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