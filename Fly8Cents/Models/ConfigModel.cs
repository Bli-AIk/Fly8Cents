using System.Collections.ObjectModel;
using ReactiveUI;

namespace Fly8Cents.Models;

public class ConfigModel : ReactiveObject
{
    private ObservableCollection<string> _blackList = [];
    private bool _isBlackHomonym;
    private bool _isWhiteHomonym;
    private ObservableCollection<string> _whiteList = [];
    private bool _isUseAi;
    private string _aiPrompt = "你是一名专业的评论审核员，请严格分析以下用户评论是否具有攻击性或侮辱性。你必须特别注意中文中利用谐音来隐藏攻击性或侮辱性意图的情况。\n\n**任务要求：**\n- 仔细阅读评论内容，识别任何直接的攻击性词语或短语。\n- 重点检查是否有使用谐音来代替敏感词汇的情况。\n- 如果评论包含任何攻击性、侮辱性、歧视性或骚扰性的内容，无论是以直接形式还是通过谐音、隐喻等方式，你的判断结果应为**true**。\n- 如果评论是中立、友好或无攻击性的，你的判断结果应为**false**。\n- 你的最终回复必须且只能是“true”或“false”，不能包含任何其他额外文字或解释。";
    private string _aiEndpoint = "https://api.deepseek.com";
    private string _aiApiKey = "";
    private string _aiModel = "deepseek-chat";

    public ObservableCollection<string> BlackList
    {
        get => _blackList;
        set => this.RaiseAndSetIfChanged(ref _blackList, value);
    }

    public ObservableCollection<string> WhiteList
    {
        get => _whiteList;
        set => this.RaiseAndSetIfChanged(ref _whiteList, value);
    }

    public bool IsWhiteHomonym
    {
        get => _isWhiteHomonym;
        set => this.RaiseAndSetIfChanged(ref _isWhiteHomonym, value);
    }

    public bool IsBlackHomonym
    {
        get => _isBlackHomonym;
        set => this.RaiseAndSetIfChanged(ref _isBlackHomonym, value);
    }
    
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
    
    public string AiEndpoint
    {
        get => _aiEndpoint;
        set => this.RaiseAndSetIfChanged(ref _aiEndpoint, value);
    }
    
    public string AiApiKey
    {
        get => _aiApiKey;
        set => this.RaiseAndSetIfChanged(ref _aiApiKey, value);
    }
    
    public string AiModel
    {
        get => _aiModel;
        set => this.RaiseAndSetIfChanged(ref _aiModel, value);
    }
}