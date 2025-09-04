using System.Linq;
using hyjiacan.py4n;

// Pinyin4NET 的命名空间

namespace Fly8Cents.Services;

/// <summary>
///     同音检查工具（静态类）
///     说明：
///     - 默认格式为 WITHOUT_TONE | WITH_V（不区分声调，ü 用 v 表示），
///     你可以通过 format 参数自定义格式（PinyinFormat 枚举在库中定义）。
/// </summary>
public static class HomophoneChecker
{
    /// <summary>
    ///     检查 shortText 的拼音序列是否连续出现在 longText 的拼音序列中，考虑到多音字。
    /// </summary>
    /// <param name="longText">较长的文本。</param>
    /// <param name="shortText">较短的文本。</param>
    /// <param name="format">拼音格式。</param>
    /// <returns>如果 shortText 的拼音连续出现在 longText 中，则返回 true；否则返回 false。</returns>
    public static bool HasHomophone(string longText,
        string shortText,
        PinyinFormat format = PinyinFormat.WITHOUT_TONE | PinyinFormat.WITH_V)
    {
        if (string.IsNullOrEmpty(longText) || string.IsNullOrEmpty(shortText))
        {
            return false;
        }

        // 舍弃长度颠倒的情况
        if (longText.Length < shortText.Length)
        {
            return false;
        }

        var arrA = Pinyin4Net.GetPinyinArray(longText, format);
        var arrB = Pinyin4Net.GetPinyinArray(shortText, format);

        // 如果 arrA 的长度小于 arrB，直接返回 false
        if (arrA.Count < arrB.Count)
        {
            return false;
        }

        // 遍历 arrA 的所有子序列
        for (var i = 0; i <= arrA.Count - arrB.Count; i++)
        {
            var match = true;
            // 检查当前子序列是否匹配 arrB
            for (var j = 0; j < arrB.Count; j++)
            {
                var pinyinA = arrA[i + j];
                var pinyinB = arrB[j];

                // 检查两个 PinyinItem 是否有任何共同的拼音
                if (pinyinA.Any(p => pinyinB.Contains(p)))
                {
                    continue;
                }

                match = false;
                break;
            }

            if (match)
            {
                return true;
            }
        }

        return false;
    }
}