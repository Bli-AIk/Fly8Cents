using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fly8Cents.Services;

public static class VideoGenerateService
{
    /// <summary>
    ///     将字符串中每行超过指定长度的部分进行换行。
    /// </summary>
    /// <param name="input">需要处理的字符串。</param>
    /// <param name="maxLength">每行允许的最大长度。默认为 25。</param>
    /// <returns>处理后的新字符串。</returns>
    public static string WrapString(string input, int maxLength = 25)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var lines = input.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
        var resultBuilder = new StringBuilder();

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // 如果行长度超过最大长度，则进行分段
            while (line.Length > maxLength)
            {
                resultBuilder.AppendLine(line[..maxLength]);
                line = line[maxLength..];
            }

            // 添加剩余的部分
            resultBuilder.Append(line);

            // 如果不是最后一行，添加换行符
            if (i < lines.Length - 1)
            {
                resultBuilder.AppendLine();
            }
        }

        return resultBuilder.ToString();
    }

    public static string GetTextPngArguments(string resolution, int textRowHeight)
    {
        var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (exeDir == null)
        {
            throw new InvalidOperationException("无法获取当前程序集所在目录。");
        }

        var pngPath = Path.Combine(exeDir, "output.png");
        var fullFontPath = Path.Combine(exeDir, "Assets/Fonts/Font.otf");
        var fullTextFile = Path.Combine(exeDir, "Comments.txt");

        var dimensions = resolution.Split('x');

        var width = int.Parse(dimensions[0]);
        var basicHeight = int.Parse(dimensions[1]);
        var fontSize = (int)(basicHeight * 0.07);
        var height = textRowHeight * (fontSize + fontSize / 15);

        // FFmpeg
        var arguments = new StringBuilder().Append($"-f lavfi -i nullsrc=s={width}x{height} -vf ")
            .Append('"')
            .Append(
                $"drawbox=t=fill,drawtext=fontfile='{fullFontPath}':textfile='{fullTextFile}':text_align=C:reload=0:")
            .Append($"x=(w-text_w)/2:y=50:fontsize={fontSize}:fontcolor=0xb89801")
            .Append('"')
            .Append(" -frames:v 1 -y ")
            .Append('"')
            .Append(pngPath)
            .Append('"')
            .ToString();

        return arguments;
    }

    public static string GetVideo2Arguments(string resolution, string preset, double duration, int frameRate)
    {
        var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (exeDir == null)
        {
            throw new InvalidOperationException("无法获取当前程序集所在目录。");
        }

        var pngPath = Path.Combine(exeDir, "output.png");
        var outputPath = Path.Combine(exeDir, "output_2.mp4");

        var dimensions = resolution.Split('x');

        var width = int.Parse(dimensions[0]);
        var height = int.Parse(dimensions[1]);

        // FFmpeg
        var arguments = new StringBuilder().Append($"-f lavfi -i nullsrc=s={resolution} -i ")
            .Append($"\"{pngPath}\"")
            .Append(" -filter_complex ")
            .Append('"')
            .Append("[0:v]drawbox=t=fill[base];")
            .Append("[base][1:v]overlay=x=0:y=main_h-(100*t)[fg];")
            .Append("[fg]perspective=1050:570:2790:570:-1800:H:W+1800:H:sense=destination[persp];")
            .Append($"[persp]drawbox=0:0:{width}:{height * 0.1}:t=fill,")
            .Append($"drawbox=0:{height - height * 0.1}:{width}:{height * 0.1}:t=fill")
            .Append('"')
            .Append($" -preset {preset} ")
            .Append("-crf 23 ")
            .Append($"-t {duration} ")
            .Append($"-r {frameRate} ")
            .Append("-y ")
            .Append('"')
            .Append(outputPath)
            .Append('"')
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