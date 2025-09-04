using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Fly8Cents.Services;

public static class ImageStitcher
{
    public static void StitchImages(int totalImageCount)
    {
        var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (exeDir == null)
        {
            throw new InvalidOperationException("无法获取当前程序集所在目录。");
        }

        StitchImages(exeDir, totalImageCount);
    }
    public static void StitchImages(string exeDir, int totalImageCount)
    {
        var imagePaths = new List<string>();
        for (var i = 0; i < totalImageCount + 1; i++)
        {
            imagePaths.Add(Path.Combine(exeDir, $"output_{i}.png"));
        }

        var loadedImages = new List<(string Path, Image<Rgba32> Img)>();

        foreach (var path in imagePaths)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"跳过：文件不存在 -> {Path.GetFileName(path)}");
                continue;
            }

            try
            {
                var img = Image.Load<Rgba32>(path);
                loadedImages.Add((path, img));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"无法加载 {Path.GetFileName(path)}: {ex.Message}");
            }
        }

        if (loadedImages.Count == 0)
        {
            Console.WriteLine("没有可用的图片可供拼接。");
            return;
        }

        // 2) 以第一张有效图片的宽度作为 commonWidth，只接受宽度一致的图片
        var commonWidth = loadedImages[0].Img.Width;
        var validImages = new List<(string Path, Image<Rgba32> Img)>();
        var totalHeight = 0;

        foreach (var pair in loadedImages)
        {
            if (pair.Img.Width != commonWidth)
            {
                Console.WriteLine($"警告：宽度不一致，跳过 {Path.GetFileName(pair.Path)} (宽度 {pair.Img.Width} != {commonWidth})");
                pair.Img.Dispose(); // 释放不符合的图片
                continue;
            }

            validImages.Add(pair);
            totalHeight += pair.Img.Height;
        }

        if (validImages.Count == 0)
        {
            Console.WriteLine("没有宽度一致的图片可供拼接。");
            return;
        }

        Console.WriteLine($"将拼接 {validImages.Count} 张图片，总高度 {totalHeight} 像素，宽度 {commonWidth} 像素。");

        var finalImagePath = Path.Combine(exeDir, "output.png");
        using (var finalImage = new Image<Rgba32>(commonWidth, totalHeight))
        {
            var offset = 0;
            finalImage.Mutate(ctx =>
            {
                foreach (var pair in validImages)
                {
                    ctx.DrawImage(pair.Img, new Point(0, offset), 1f);
                    offset += pair.Img.Height;
                }
            });

            foreach (var pair in validImages)
            {
                pair.Img.Dispose();
            }


            finalImage.Save(finalImagePath);
        }

        Console.WriteLine($"图片已成功拼接并保存到：{finalImagePath}");
    }
    
    public static void DeleteImages(int totalImageCount)
    {
        var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (exeDir == null)
        {
            throw new InvalidOperationException("无法获取当前程序集所在目录。");
        }

        DeleteImages(exeDir, totalImageCount);
    }
    public static void DeleteImages(string exeDir, int totalImageCount)
    {
        var imagePaths = new List<string>();
        for (var i = 0; i < totalImageCount + 1; i++)
        {
            imagePaths.Add(Path.Combine(exeDir, $"output_{i}.png"));
        }
        foreach (var path in imagePaths)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Console.WriteLine($"已删除：{Path.GetFileName(path)}");
                }
                else
                {
                    Console.WriteLine($"文件不存在，跳过：{Path.GetFileName(path)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除文件 {Path.GetFileName(path)} 时出错：{ex.Message}");
            }
        }
    }
}