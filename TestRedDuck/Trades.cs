using ExchangeSharp;
using System;
using Telegram.Bot.Types.ReplyMarkups;

namespace TestRedDuck
{
    partial class TGBot
    {

        private IWebSocket trades = null;

        private int botMsgTradesId = 0;

        private async void GetTrades(Telegram.Bot.Types.Message lastMessage) // Вывод трейдов
        {

            await bot.SendTextMessageAsync(lastMessage.Chat.Id, "Result:", replyMarkup: new ReplyKeyboardRemove());

            TakeAPI(lastMessage.Chat.Id);

            TakeSymbol(lastMessage.Chat.Id);

            try
            {
                if (trades != null)
                    trades.Dispose();

                botMsgTradesId = 0;

                trades = await api.GetTradesWebSocketAsync(async m =>
                {
                    try
                    {

                        await bot.EditMessageTextAsync
                        (
                            lastMessage.Chat.Id,
                            botMsgTradesId,
                            $"{exchange} - {gSymbol}: Amount - {m.Value.Amount}; Price - {m.Value.Price}; Time - {m.Value.Timestamp.ToLocalTime()}");
                    }
                    catch
                    {

                        botMsgTradesId = (await bot.SendTextMessageAsync
                        (
                            lastMessage.Chat.Id,
                            $"{exchange} - {gSymbol}: Amount - {m.Value.Amount}; Price - {m.Value.Price}; Time - {m.Value.Timestamp.ToLocalTime()}")).MessageId;
                    }
                }, mSymbol);

            }

            catch (Exception e)
            {

                Console.WriteLine(e.Message.ToString());
            }

        }

    }
}
