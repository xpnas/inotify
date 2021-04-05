
using System;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;

namespace Inotify.Sends.Products
{

    public class TelegramBotAuth
    {
        [InputTypeAttribte(0, "BotToken", "BotToken", "ID:Token")]
        public string BotToken { get; set; }

        [InputTypeAttribte(1, "Chat_id", "ChatId", "ChatId")]
        public string Chat_id { get; set; }
    }


    [SendMethodKey("E9669473-FF0B-4474-92BB-E939D92045BB", "电报机器人", Order = 2)]

    public class TelegramBotSendTemplate : SendTemplate<TelegramBotAuth>
    {


        public override bool SendMessage(SendMessage message)
        {

            var proxy = GetProxy();
            var client = proxy == null ? new TelegramBotClient(Auth.BotToken) : new TelegramBotClient(Auth.BotToken, proxy);
            var content = string.IsNullOrEmpty(message.Title) ? message.Title : message.Title + "\n" + message.Data;
            var isIMG = !string.IsNullOrEmpty(message.Title) && IsUrl(message.Title) && IsImage(message.Title) && string.IsNullOrEmpty(message.Data);
            if (isIMG)
            {
                client.SendPhotoAsync(Auth.Chat_id, new InputOnlineFile(new Uri(message.Title)));
            }
            else
            {
                client.SendTextMessageAsync(Auth.Chat_id, content);
            }
            return true;
        }
    }
}
