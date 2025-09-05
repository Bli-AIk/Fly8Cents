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

    public static string GetTextPngArguments(string resolution, string text, int lineCount)
    {
        var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (exeDir == null)
        {
            throw new InvalidOperationException("无法获取当前程序集所在目录。");
        }

        var pngPath = Path.Combine(exeDir, $"output_{lineCount}.png");
        var fullFontPath = Path.Combine(exeDir, "Assets/Fonts/Font.otf");

        var dimensions = resolution.Split('x');

        var width = int.Parse(dimensions[0]);
        var basicHeight = int.Parse(dimensions[1]);
        var fontSize = (int)(basicHeight * 0.07);
        var height = (int)(fontSize * 1.067);

        // FFmpeg
        var arguments = new StringBuilder().Append($"-f lavfi -i nullsrc=s={width}x{height} -vf ")
            .Append('"')
            .Append(
                $"drawbox=t=fill,drawtext=fontfile='{fullFontPath}':text='{EscapeForDrawText(text)}':text_align=C:reload=0:")
            .Append($"x=(w-text_w)/2:fontsize={fontSize}:fontcolor=0xb89801")
            .Append('"')
            .Append(" -frames:v 1 -y ")
            .Append('"')
            .Append(pngPath)
            .Append('"')
            .ToString();

        return arguments;
    }

    public static string GetVideo1Arguments(string resolution, string preset, int frameRate, string text)
    {
        var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (exeDir == null)
        {
            throw new InvalidOperationException("无法获取当前程序集所在目录。");
        }

        var outputPath = Path.Combine(exeDir, "output_1.mp4");
        var fullFontPath = Path.Combine(exeDir, "Assets/Fonts/Font.otf");

        var dimensions = resolution.Split('x');

        var height = int.Parse(dimensions[1]);
        var fontSize = (int)(height * 0.07);
        
        // FFmpeg
        var arguments = new StringBuilder().Append($"-f lavfi -i nullsrc=s={resolution} -vf ")
            .Append('"')
            .Append("drawbox=t=fill,")
            .Append($"drawtext=fontfile='{fullFontPath}':text='{text}':text_align=C:reload=0:")
            .Append($"x=(w-text_w)/2:y=({height}-text_h)/2:")
            .Append($@"fontsize={fontSize}*if(lt(t\,5)\,2\,if(lt(t\,8)\,2-(t-5)*(2-0.25)/3\,0.25)):")
            .Append(@"fontcolor=0xb89801:alpha=if(lt(t\,2)\,0\,if(lt(t\,4)\,(t-2)/2\,if(lt(t\,7)\,1\,if(lt(t\,8)\,1-(t-7)/1\,0))))")
            .Append('"')
            .Append($" -preset {preset} ")
            .Append("-crf 23 ")
            .Append("-t 8 ")
            .Append($"-r {frameRate} ")
            .Append("-y ")
            .Append('"')
            .Append(outputPath)
            .Append('"')
            .Append(" -progress pipe:1")
            .ToString();

        return arguments;
    }

    public static string GetVideo2Arguments(string resolution, string preset, int frameRate, double duration)
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

        var scaleX = width / 3840.0;
        var scaleY = height / 2160.0;
        var perspX0 = (int)(1050 * scaleX);
        const int perspY = 570 / 4;
        var perspY0 = (int)(perspY * scaleY);
        var perspX1 = (int)(2790 * scaleX);
        var perspY1 = (int)(perspY * scaleY);
        var perspX2 = (int)(1800 * scaleX);
        
        var speed = (int)(100 * scaleY);
        // FFmpeg
        var arguments = new StringBuilder().Append($"-f lavfi -i nullsrc=s={resolution} -i ")
            .Append($"\"{pngPath}\"")
            .Append(" -filter_complex ")
            .Append('"')
            .Append("[0:v]drawbox=t=fill[base];")
            .Append($"[base][1:v]overlay=x=0:y=main_h-({speed}*t)[fg];")
            .Append($"[fg]perspective={perspX0}:{perspY0}:{perspX1}:{perspY1}:-{perspX2}:H:W+{perspX2}:")
            .Append("H:sense=destination[persp];")
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
    
    public static string GetVideo3Arguments(string resolution, string preset, int frameRate, string text)
    {
        var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (exeDir == null)
        {
            throw new InvalidOperationException("无法获取当前程序集所在目录。");
        }

        var outputPath = Path.Combine(exeDir, "output_3.mp4");
        var fullFontPath = Path.Combine(exeDir, "Assets/Fonts/Font.otf");

        var dimensions = resolution.Split('x');

        var height = int.Parse(dimensions[1]);
        var fontSize = (int)(height * 0.07);
        
        // FFmpeg
        var arguments = new StringBuilder().Append($"-f lavfi -i nullsrc=s={resolution} -vf ")
            .Append('"')
            .Append("drawbox=t=fill,")
            .Append($"drawtext=fontfile='{fullFontPath}':text='{text}':text_align=C:reload=0:")
            // 尊重沈美老师，text_h*2
            .Append($"x=(w-text_w)/2:y=({height}-text_h*2)/2:")
            .Append($"fontsize={fontSize}*3:")
            .Append("fontcolor=0xb89801:alpha='min(t,1)'")
            .Append('"')
            .Append($" -preset {preset} ")
            .Append("-crf 23 ")
            .Append("-t 10 ")
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

    private static string EscapeForDrawText(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var sb = new StringBuilder();

        foreach (var c in input)
        {
            switch (c)
            {
                case ':':
                    sb.Append("\\:"); // FFmpeg 参数分隔符
                    break;
                case '\'':
                    sb.Append("\\'"); // 单引号
                    break;
                case '\"':
                    sb.Append("\\\""); // 双引号
                    break;
                case '\\':
                    sb.Append(@"\\"); // 反斜杠
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        return sb.ToString();
    }
}