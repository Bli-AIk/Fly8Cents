using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;

namespace Fly8Cents.ViewModels;

public class FilterViewModel : ViewModelBase
{
    private bool _aiCheck;
    private string _aiPrompt = "你是一名专业的评论审核员，请严格分析以下用户评论是否具有攻击性或侮辱性。你必须特别注意中文中利用谐音来隐藏攻击性或侮辱性意图的情况。\n\n---\n**用户评论：**\n[用户评论内容]\n---\n\n**任务要求：**\n- 仔细阅读评论内容，识别任何直接的攻击性词语或短语。\n- 重点检查是否有使用谐音来代替敏感词汇的情况。例如，用“小粉红”的谐音“xiao fen hong”来指代特定人群。\n- 如果评论包含任何攻击性、侮辱性、歧视性或骚扰性的内容，无论是以直接形式还是通过谐音、隐喻等方式，你的判断结果应为**true**。\n- 如果评论是中立、友好或无攻击性的，你的判断结果应为**false**。\n- 你的最终回复必须且只能是“true”或“false”，不能包含任何其他额外文字或解释。\n\n**你的回复：**";

    private ObservableCollection<string> _blackList = [];

    private string _newBlackItem = "";

    private string _newWhiteItem = "";
    private string? _selectedBlackItem;
    private string? _selectedWhiteItem;

    private ObservableCollection<string> _whiteList = [];

    public FilterViewModel()
    {
        AddBlackCommand = ReactiveCommand.Create(() =>
        {
            if (string.IsNullOrWhiteSpace(NewBlackItem))
            {
                return;
            }

            if (BlackList.Any(item => item == NewWhiteItem))
            {
                return;
            }

            BlackList.Add(NewBlackItem);
            NewBlackItem = "";
        });

        RemoveBlackCommand = ReactiveCommand.Create(() =>
        {
            if (!string.IsNullOrWhiteSpace(SelectedBlackItem))
            {
                BlackList.Remove(SelectedBlackItem);
            }
        });

        ClearBlackCommand = ReactiveCommand.Create(() =>
        {
            BlackList.Clear();
        });
        
        AddWhiteCommand = ReactiveCommand.Create(() =>
        {
            if (string.IsNullOrWhiteSpace(NewWhiteItem))
            {
                return;
            }

            if (WhiteList.Any(item => item == NewWhiteItem))
            {
                return;
            }

            WhiteList.Add(NewWhiteItem);
            NewWhiteItem = "";
        });

        RemoveWhiteCommand = ReactiveCommand.Create(() =>
        {
            if (!string.IsNullOrWhiteSpace(SelectedWhiteItem))
            {
                WhiteList.Remove(SelectedWhiteItem);
            }
        });
        
        ClearWhiteCommand = ReactiveCommand.Create(() =>
        {
            WhiteList.Clear();
        });
    }

    public ObservableCollection<string> BlackList
    {
        get => _blackList;
        set => this.RaiseAndSetIfChanged(ref _blackList, value);
    }

    public string NewBlackItem
    {
        get => _newBlackItem;
        set => this.RaiseAndSetIfChanged(ref _newBlackItem, value);
    }

    public ObservableCollection<string> WhiteList
    {
        get => _whiteList;
        set => this.RaiseAndSetIfChanged(ref _whiteList, value);
    }

    public string NewWhiteItem
    {
        get => _newWhiteItem;
        set => this.RaiseAndSetIfChanged(ref _newWhiteItem, value);
    }

    public bool AiCheck
    {
        get => _aiCheck;
        set => this.RaiseAndSetIfChanged(ref _aiCheck, value);
    }

    public string AiPrompt
    {
        get => _aiPrompt;
        set => this.RaiseAndSetIfChanged(ref _aiPrompt, value);
    }

    public string? SelectedWhiteItem
    {
        get => _selectedWhiteItem;
        set => this.RaiseAndSetIfChanged(ref _selectedWhiteItem, value);
    }

    public string? SelectedBlackItem
    {
        get => _selectedBlackItem;
        set => this.RaiseAndSetIfChanged(ref _selectedBlackItem, value);
    }


    public ReactiveCommand<Unit, Unit> AddBlackCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveBlackCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearBlackCommand { get; }
    public ReactiveCommand<Unit, Unit> AddWhiteCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveWhiteCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearWhiteCommand { get; }
}
