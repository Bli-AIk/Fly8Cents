# 8月名单生成器

## 简介
一个开源工具，用于快速提取视频网站评论区的友善言论，并获取发布时间和发布者等信息，最终生成《星球大战》片头风格的视频。

灵感来自 B站up主 “沈阳美食家” 的 [神人视频](https://www.bilibili.com/video/BV1c1a3zPEH1/)。

## 示例
[![【完整名单】1-9月评论区羞辱过沈阳美食家的人，以及回应](https://i0.hdslb.com/bfs/archive/6c2bafd235cb2bbaaec959b3537c94894a63297b.jpg)](https://www.bilibili.com/video/BV1wRa4zVEWW)

## 功能

* 按规则提取 哔哩哔哩 网站中指定Up主发布视频/动态的 评论区评论。
* 生成带有音乐的星战片头风格视频……（音乐可自行替换）
* ……还有不明所以的片头和片尾。
* 支持自定义文本。

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

衷心感谢你们！

## 待办
* 支持 AI 集成筛选

## 贡献
欢迎提交 issue 或 PR，让生成器更好用。

## 许可证
GPL-3.0 license