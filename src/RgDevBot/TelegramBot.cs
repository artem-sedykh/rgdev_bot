using System;
using Telegram.Bot;

namespace RgDevBot
{
    public class TelegramBot
    {
        private readonly TelegramBotClient _telegram;
        public long MainChatId;

        public TelegramBot()
        {
            var token = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN");
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("Please define TELEGRAM_TOKEN variable.");
                
            }

            long.TryParse(Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID"), out var chatId);
            if (chatId == 0)
            {
                throw new Exception("Please define TELEGRAM_CHAT_ID variable.");
            }

            MainChatId = chatId;
            _telegram = new TelegramBotClient(token);
            _telegram.OnMessage += Bot_OnMessage;
            _telegram.StartReceiving();
        }

        public void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            Console.WriteLine($"[{e.Message.Chat.Id}, user: {e.Message.From.Id}] @{e.Message.From?.Username}: {e.Message.Text}");
        }

        public void SendMessage(string message, long? chatId = null)
        {
            _telegram.SendTextMessageAsync(chatId ?? MainChatId, message).GetAwaiter().GetResult();
        }
    }
}