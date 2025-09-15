using System;
using System.ClientModel;
using System.Net.Mime;
using OpenAI;
using OpenAI.Chat;

namespace Fly8Cents.Services
{
    public class AiService
    {
        public static string CheckCommentAsync(string message, string endpoint, string apiKey, string model = "deepseek-ai/DeepSeek-V3")
        {
            var openAiClientOptions = new OpenAIClientOptions();
            openAiClientOptions.Endpoint = new Uri(endpoint);
            OpenAIClient openAiClient = new(
                new ApiKeyCredential(apiKey),
                openAiClientOptions);
            var chatClient = openAiClient.GetChatClient(model);

            try
            {

                ChatCompletion completion = chatClient.CompleteChat(
                [
                    new SystemChatMessage("你是一名专业的评论审核员，请严格分析以下用户评论是否具有攻击性或侮辱性。你必须特别注意中文中利用谐音来隐藏攻击性或侮辱性意图的情况。\n\n**任务要求：**\n- 仔细阅读评论内容，识别任何直接的攻击性词语或短语。\n- 重点检查是否有使用谐音来代替敏感词汇的情况。\n- 如果评论包含任何攻击性、侮辱性、歧视性或骚扰性的内容，无论是以直接形式还是通过谐音、隐喻等方式，你的判断结果应为**true**。\n- 如果评论是中立、友好或无攻击性的，你的判断结果应为**false**。\n- 你的最终回复必须且只能是“true”或“false”，不能包含任何其他额外文字或解释。"),
                    new UserChatMessage($"**用户评论：**\n{message}"),
                    new AssistantChatMessage("**你的审核结果：**\n")
                ]);
                Console.WriteLine($"{completion.Role}: {completion.Content[0].Text}");
                return completion.Content[0].Text;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "";
            }
        }
    }
}