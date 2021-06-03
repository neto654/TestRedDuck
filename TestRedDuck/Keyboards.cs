using Telegram.Bot.Types.ReplyMarkups;

namespace TestRedDuck
{
    partial class TGBot
    {

        private ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup();

        private string exchange = "";
        private string gSymbol = "";
        private string mSymbol = "";

        private async void KeyExchange(Telegram.Bot.Types.Message lastMessage) // Выбор биржи
        {

            keyboard = new ReplyKeyboardMarkup
            {

                Keyboard = new[]
                        {

                            new[]
                            {

                                new KeyboardButton("Binance"),
                                new KeyboardButton("Kraken")
                            },
                        },

                ResizeKeyboard = true
            };

            await bot.SendTextMessageAsync(lastMessage.Chat.Id, "Select exchange ->", replyMarkup: keyboard);
        }

        private async void KeyPair(Telegram.Bot.Types.Message lastMessage) // Выбор пары
        {

            exchange = lastMessage.Text;

            keyboard = new ReplyKeyboardMarkup
            {

                Keyboard = new[]
                        {

                            new[]
                            {

                                new KeyboardButton("BTC-USDT"),
                                new KeyboardButton("ETH-BTC"),
                                new KeyboardButton("EUR-USD"),
                                new KeyboardButton("USDT-BTC")
                            },
                        },

                ResizeKeyboard = true
            };

            await bot.SendTextMessageAsync(lastMessage.Chat.Id, "Select global symbol ->", replyMarkup: keyboard);
        }

        private async void KeyData(Telegram.Bot.Types.Message lastMessage) // Выбор операции
        {

            gSymbol = lastMessage.Text;

            keyboard = new ReplyKeyboardMarkup
            {

                Keyboard = new[]
                        {

                            new[]
                            {

                                new KeyboardButton("Trades"),
                                new KeyboardButton("Candles"),
                            },
                        },

                ResizeKeyboard = true
            };

            await bot.SendTextMessageAsync(lastMessage.Chat.Id, "Select data ->", replyMarkup: keyboard);
        }

        private async void KeyStop(Telegram.Bot.Types.Message lastMessage) // Остановка обновлений
        {

            if (trades != null)
                trades.Dispose();
            if (webSocket != null)
                webSocket.Dispose();

            await bot.SendTextMessageAsync(lastMessage.Chat.Id, "Updates stopped");
        }
    }
}
