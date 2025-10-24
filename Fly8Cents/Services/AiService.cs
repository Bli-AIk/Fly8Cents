using System;
using System.ClientModel;
using System.Net.Mime;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;

namespace Fly8Cents.Services
{
    public static class AiService
    {
        public static async Task<string> CheckCommentAsync(
            string aiPrompt,
            string userChatMessage,
            string endpoint,
            string apiKey,
            string model)
        {
            var openAiClientOptions = new OpenAIClientOptions
            {
                Endpoint = new Uri(endpoint)
            };
            OpenAIClient openAiClient = new(
                new ApiKeyCredential(apiKey),
                openAiClientOptions);
            var chatClient = openAiClient.GetChatClient(model);

            try
            {
                ChatCompletion completion = await chatClient.CompleteChatAsync(
                    new SystemChatMessage(aiPrompt),
                    new UserChatMessage($"**用户评论：**\n{userChatMessage}"),
                    new AssistantChatMessage("**你的审核结果：**\n")
                );
                Console.WriteLine($"{completion.Role}: {completion.Content[0].Text}");
                return completion.Content[0].Text;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return string.Empty;
            }
        }
    }
}