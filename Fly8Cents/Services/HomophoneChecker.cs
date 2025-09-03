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
    ///     检查 textB 的拼音序列是否连续出现在 textA 的拼音序列中，考虑到多音字。
    /// </summary>
    /// <param name="textA">较长的文本。</param>
    /// <param name="textB">较短的文本。</param>
    /// <param name="format">拼音格式。</param>
    /// <returns>如果 textB 的拼音连续出现在 textA 中，则返回 true；否则返回 false。</returns>
    public static bool HasHomophone(string textA,
        string textB,
        PinyinFormat format = PinyinFormat.WITHOUT_TONE | PinyinFormat.WITH_V)
    {
        if (string.IsNullOrEmpty(textA) || string.IsNullOrEmpty(textB))
        {
            return false;
        }

        // 确保 arrA 总是较长或长度相等
        if (textA.Length < textB.Length)
        {
            (textA, textB) = (textB, textA);
        }

        var arrA = Pinyin4Net.GetPinyinArray(textA, format);
        var arrB = Pinyin4Net.GetPinyinArray(textB, format);

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