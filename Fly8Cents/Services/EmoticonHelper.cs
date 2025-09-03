using System;
using System.Text.RegularExpressions;

namespace Fly8Cents.Services;

public static partial class EmoticonHelper
{
    [GeneratedRegex(@"\[(.+?)\]", RegexOptions.CultureInvariant)]
    private static partial Regex BiliEmoticonRegex();

    /// <summary>
    /// [汤圆] -75%> (还有个汤圆)
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