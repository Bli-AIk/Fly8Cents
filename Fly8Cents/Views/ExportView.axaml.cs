using Avalonia.Controls;

namespace Fly8Cents.Views;

public partial class ExportView : UserControl
{
    public ExportView()
    {
        InitializeComponent();
        ConfigTips.Text = """
                          {year}：年份（4位数，补零，如 2025）
                          
                          {month}：月份（2位数，补零，如 09）
                          
                          {day}：日期（2位数，补零，如 06）
                          
                          {hour}：小时（24小时制，2位数，补零，如 08）
                          
                          {minute}：分钟（2位数，补零，如 05）
                          
                          {uname}：用户昵称
                          
                          {mid}：用户唯一 ID（数字）
                          
                          {sign}：用户个性签名
                          
                          {level}：用户等级
                          
                          {sex}：用户性别（男 / 女 / 保密）
                          
                          {like}：评论点赞数
                          
                          {message}：评论内容
                          """;
    }
}