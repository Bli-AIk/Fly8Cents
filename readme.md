# 8月名单生成器

## 简介
一个开源工具，用于快速提取视频网站评论区的友善言论，并获取发布时间和发布者等信息，最终生成《星球大战》片头风格的视频。

灵感来自 B站up主 “沈阳美食家” 的 [神人视频](https://www.bilibili.com/video/BV1c1a3zPEH1/)。

## 示例
【完整名单】1-9月评论区羞辱过沈阳美食家的人，以及回应    --哔哩哔哩
[![【完整名单】1-9月评论区羞辱过沈阳美食家的人，以及回应](https://i0.hdslb.com/bfs/archive/f1cce609b210eb7a8cb4080859aa49f5e75a6a66.jpg)](https://www.bilibili.com/video/BV1M6a4zXEcM)

## 功能

* 按规则提取 哔哩哔哩 网站中指定Up主发布视频/动态的 评论区评论。
* 生成带有音乐的星战片头风格视频……（音乐可自行替换）
* ……还有不明所以的片头和片尾。
* 支持自定义文本。
* 支持通过开放接口（Open API，包括 DeepSeek、OpenAI 等）调用 AI 进行筛选。

## 快速开始

本项目依赖 [FFmpeg](https://ffmpeg.org/) 进行视频导出，但 **不影响评论爬取功能**。
如果你只需要爬取评论，可以跳过 FFmpeg 的安装。

### 1. 安装 FFmpeg（可选，仅用于视频导出）

* **Windows**

    1. 前往 [FFmpeg 下载页](https://ffmpeg.org/download.html) 获取 Windows 版本。
    2. 解压缩后，将 `bin` 目录路径加入到系统 **环境变量 `PATH`** 中。
    3. 打开命令行，输入 `ffmpeg -version` 验证是否安装成功。

* **Linux**

    * **Arch Linux / Manjaro**

      ```bash
      sudo pacman -S ffmpeg
      ffmpeg -version
      ```
    * **Debian / Ubuntu / 其他基于 Debian 的发行版**

      ```bash
      sudo apt update
      sudo apt install ffmpeg
      ffmpeg -version
      ```

* **macOS**
  使用 [Homebrew](https://brew.sh/) 安装：

  ```bash
  brew install ffmpeg
  ffmpeg -version
  ```

### 2. 下载本项目软件

1. 根据你的操作系统，前往 [Releases](./releases) 页面下载对应的发行版。
2. 解压后直接运行可执行文件即可。

## 依赖项 / 致谢
本项目基于以下库而制作：
- [AvaloniaUI](https://avaloniaui.net/)
- [Avalonia.Labs](https://github.com/AvaloniaUI/Avalonia.Labs)
- [FFmpeg](https://github.com/FFmpeg/FFmpeg)
- [bilibili-API-collect](https://github.com/SocialSisterYi/bilibili-API-collect)
- [quicktype](https://github.com/glideapps/quicktype)
- [Pinyin4NET](https://github.com/hyjiacan/Pinyin4NET)
- [ImageSharp](https://github.com/SixLabors/ImageSharp)

本项目参考了以下学习资料：
- [How to make a Star Wars scrolling / crawl opening text video in 4k | Video crawler](https://www.youtube.com/watch?v=ee-p815fLYM&ab_channel=TheFFMPEGguy)
- [Fade In and Out text using the 'drawtext' filter](https://ffmpegbyexample.com/examples/50gowmkq/fade_in_and_out_text_using_the_drawtext_filter/)

本项目中默认附带的音频文件：
- [CD 1_Charlie Chaplin - Pay day [1922].](https://archive.org/details/charlie-chaplin-the-essential-film-music-collection-2006-opus-128)

本项目中默认附带的字体文件：
- [思源黑体](https://github.com/adobe-fonts/source-han-sans)

衷心感谢你们！

## 贡献
以下人员为本项目做出了贡献。

<a href = "https://github.com/Bli-AIk/Fly8Cents/Python/graphs/contributors">
<img src = "https://contrib.rocks/image?repo=Bli-AIk/Fly8Cents"/>
</a>

**衷心感谢你们每一个人！🎔**

如果你发现Bug、想要修复问题或者改进此项目，欢迎提交 issue 或 PR，让生成器更好用。

## 许可证
GPL-3.0 license
