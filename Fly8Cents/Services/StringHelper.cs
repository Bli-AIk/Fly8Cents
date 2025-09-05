using System;
using System.Text.RegularExpressions;

namespace Fly8Cents.Services;

public static partial class StringHelper
{
    public static string FormatComment(DateTimeOffset dateTime,
        string uname,
        long mid,
        string sign,
        long level,
        string sex,
        long like,
        string message,
        string template)
    {
        return template
            .Replace("{year}", dateTime.Year.ToString("D4"))
            .Replace("{month}", dateTime.Month.ToString("D2"))
            .Replace("{day}", dateTime.Day.ToString("D2"))
            .Replace("{hour}", dateTime.Hour.ToString("D2"))
            .Replace("{minute}", dateTime.Minute.ToString("D2"))
            .Replace("{uname}", uname)
            .Replace("{mid}", mid.ToString())
            .Replace("{sign}", sign)
            .Replace("{level}", level.ToString())
            .Replace("{sex}", sex)
            .Replace("{like}", like.ToString())
            .Replace("{message}", message);
    }


    [GeneratedRegex(@"\[(.+?)\]", RegexOptions.CultureInvariant)]
    private static partial Regex BiliEmoticonRegex();

    /// <summary>
    ///     [汤圆] -75%> (还有个汤圆)
    /// </summary>
    public static string ProcessBilibiliEmoticon(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var matches = BiliEmoticonRegex().Matches(input);

        if (matches.Count != 1)
        {
            return input;
        }

        var innerText = matches[0].Groups[1].Value;

        return Random.Shared.NextDouble() < 0.75
            ? BiliEmoticonRegex().Replace(input, $"(还有个{innerText})")
            : input;
    }
}