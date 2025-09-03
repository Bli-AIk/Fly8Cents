using ReactiveUI;

namespace Fly8Cents.ViewModels;

public class TextSettingsViewModel : ViewModelBase
{
    private bool _isUseAi;
    private string _startText = "8月评论区\n羞辱过我的人";
    private string _endText = "反弹！";
    private string _aiPrompt =
        "你是一名专业的评论审核员，请严格分析以下用户评论是否具有攻击性或侮辱性。你必须特别注意中文中利用谐音来隐藏攻击性或侮辱性意图的情况。\n\n---\n**用户评论：**\n[用户评论内容]\n---\n\n**任务要求：**\n- 仔细阅读评论内容，识别任何直接的攻击性词语或短语。\n- 重点检查是否有使用谐音来代替敏感词汇的情况。\n- 如果评论包含任何攻击性、侮辱性、歧视性或骚扰性的内容，无论是以直接形式还是通过谐音、隐喻等方式，你的判断结果应为**true**。\n- 如果评论是中立、友好或无攻击性的，你的判断结果应为**false**。\n- 你的最终回复必须且只能是“true”或“false”，不能包含任何其他额外文字或解释。\n\n**你的回复：**";

    public bool IsUseAi
    {
        get => _isUseAi;
        set => this.RaiseAndSetIfChanged(ref _isUseAi, value);
    }

    public string AiPrompt
    {
        get => _aiPrompt;
        set => this.RaiseAndSetIfChanged(ref _aiPrompt, value);
    }
    public string StartText
    {
        get => _startText;
        set => this.RaiseAndSetIfChanged(ref _startText, value);
    }

    public string EndText
    {
        get => _endText;
        set => this.RaiseAndSetIfChanged(ref _endText, value);
    }
}