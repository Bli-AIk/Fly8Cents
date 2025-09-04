using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fly8Cents.Services;

public static class VideoGenerateService
{
    public static string GetVideo2Arguments(string resolution, string preset, double duration, int frameRate)
    {
        var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
       
        if (exeDir == null)
        {
            throw new InvalidOperationException("无法获取当前程序集所在目录。");
        }

        var fullFontPath = Path.Combine(exeDir, "Assets/Fonts/Font.otf");
        var fullTextFile = Path.Combine(exeDir, "Comments.txt");
        var outputPath = Path.Combine(exeDir, "output_2.mp4");

        var dimensions = resolution.Split('x');

        var width = int.Parse(dimensions[0]);
        var height = int.Parse(dimensions[1]);

        // 根据高度调整字体大小，例如：高度的7%
        var fontSize = (int)(height * 0.07);

        // FFmpeg
        var arguments = new StringBuilder().Append($"-f lavfi -i nullsrc=s={resolution} -vf ")
            .Append("drawbox=t=fill,")
            .Append($"drawtext={fullFontPath}:")
            .Append($"textfile={fullTextFile}:")
            .Append($"x=(w-text_w)/2:y=h-100*t:fontsize={fontSize}:fontcolor=0xb89801,")
            .Append("drawbox,")
            .Append("perspective=1050:570:2790:570:-1800:H:W+1800:H:sense=destination,")
            .Append($"drawbox=0:0:{width}:{height * 0.1}:t=fill,")
            .Append($"drawbox=0:{height - height * 0.1}:{width}:{height * 0.1}:t=fill ")
            .Append($"-preset {preset} ")
            .Append("-crf 23 ")
            .Append($"-t {duration} ")
            .Append($"-r {frameRate} ")
            .Append("-y ")
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