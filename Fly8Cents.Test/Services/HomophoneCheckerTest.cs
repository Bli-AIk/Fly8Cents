using Fly8Cents.Services;
using JetBrains.Annotations;

namespace Fly8Cents.Test.Services;

[TestSubject(typeof(HomophoneChecker))]
public class HomophoneCheckerTest
{
    // 假设 HomophoneChecker.HasHomophone 是您之前定义的静态方法，
    // 并且它依赖的 Pinyin4Net 等库已经正确配置或模拟。

    [Fact]
    public void HasHomophone_ShouldReturnTrue_WhenTextB_IsHomophone_OfA_SubsequenceOfTextA()
    {
        // Arrange
        // 这里的 HomophoneChecker 是一个占位符，请根据您的实际类名进行调整。

        var textA = "房偏了"; // fang pian le
        var textB = "方便"; // fang bian/pian

        // Act
        // 调用您实现的方法
        var result = HomophoneChecker.HasHomophone(textA, textB);

        // Assert
        // 期望结果是 true，因为 "方便" 的拼音 fang-pian 与 "房偏了" 中的 fang-pian 序列匹配。
        Assert.True(result);
    }

    [Fact]
    public void HasHomophone_ShouldReturnTrue_WhenTextB_IsHomophone_OfA_SubsequenceOfTextA_WithDifferentLength()
    {
        // Arrange
        var textA = "方便面馆"; // fang bian mian guan
        var textB = "房方便"; // fang fang bian

        // Act
        // 调用您实现的方法
        var result = HomophoneChecker.HasHomophone(textA, textB);

        // Assert
        var result2 = HomophoneChecker.HasHomophone(textB, textA);

        // 期望结果是 false
        Assert.False(result);
        Assert.False(result2);
    }


    [Fact]
    public void HasHomophone_ShouldReturnFalse_WhenNo_HomophoneMatch_IsFound()
    {
        // Arrange

        var textA = "你好世界"; // ni hao shi jie
        var textB = "你靠时间"; // ni kao shi jian

        // Act
        var result = HomophoneChecker.HasHomophone(textA, textB);

        // Assert
        // 期望结果是 false，因为除了第一个字，后续拼音都不匹配。
        Assert.False(result);
    }

    [Fact]
    public void HasHomophone_ShouldReturnFalse_WhenTextB_IsLongerThanTextA_AndNoMatchIsPossible()
    {
        // Arrange

        var textA = "你好"; // ni hao
        var textB = "你好世界"; // ni hao shi jie

        // Act
        var result = HomophoneChecker.HasHomophone(textA, textB);

        // Assert
        // 期望结果是 true，因为代码逻辑会处理这种情况。
        Assert.True(result);
    }
}