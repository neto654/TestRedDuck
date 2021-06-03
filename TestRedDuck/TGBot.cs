using System;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace TestRedDuck
{
    partial class TGBot
    {

        private TelegramBotClient bot;

        public TGBot()
        {
            
            AllMessages();

            string token = File.ReadAllText("token"); // Получить токен из файла

            bot = new TelegramBotClient(token); // Создать бота

            bot.OnMessage += Bot_OnMessage; // Подписка на событие

            bot.StartReceiving(); // Старт бота
        }

        private async void Bot_OnMessage(object sender, MessageEventArgs e) // Обработка сообщений
        {

            var msg = e.Message.Text;
            if (msg == null)
                return;

            try
            {
                MsgFromUsers[msg.ToLower()](e.Message);
            }
            catch (Exception)
            {
                var msgFail = new Exception("Don't find this command!!");
                await bot.SendTextMessageAsync(e.Message.Chat.Id, msgFail.Message.ToString());
            }

        }

    }

}
