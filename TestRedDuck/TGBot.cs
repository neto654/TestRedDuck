using ExchangeSharp;
using System;
using System.IO;
using System.Linq;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace TestRedDuck
{
    class TGBot
    {

        private TelegramBotClient bot;

        private string exchange = "";
        private string gSymbol = "";

        private IWebSocket trades = null;

        private int botMsgTradesId = 0;
        private int botMsgCandlesId = 0;

        private Timer timerCandles = new Timer();

        public TGBot()
        {

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

            var keyboard = new ReplyKeyboardMarkup();

            switch (msg.ToLower())
            {

                case "/start":
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

                    await bot.SendTextMessageAsync(e.Message.Chat.Id, "Select exchange ->", replyMarkup: keyboard);

                    break;

                case "binance":
                case "kraken":
                    exchange = msg;

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

                    await bot.SendTextMessageAsync(e.Message.Chat.Id, "Select global symbol ->", replyMarkup: keyboard);

                    break;

                case "usdt-btc":
                case "btc-usdt":
                case "eth-btc":
                case "eur-usd":
                    gSymbol = msg;

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

                    await bot.SendTextMessageAsync(e.Message.Chat.Id, "Select data ->", replyMarkup: keyboard);

                    break;

                case "trades":
                    await bot.SendTextMessageAsync(e.Message.Chat.Id, "Result:", replyMarkup: new ReplyKeyboardRemove());
                    showTrades(exchange, gSymbol, e.Message.Chat.Id);
                    break;

                case "candles":
                    await bot.SendTextMessageAsync(e.Message.Chat.Id, "Result:", replyMarkup: new ReplyKeyboardRemove());
                    showCandles(exchange, gSymbol, e.Message.Chat.Id);
                    break;

                case "/stop":
                    if (timerCandles.Enabled)
                        timerCandles.Stop();
                    if (trades != null)
                        trades.Dispose();

                    await bot.SendTextMessageAsync(e.Message.Chat.Id, "Updates stopped");

                    break;

            }

        }

        private async void showTrades(string exchange, string gSymbol, long chatId) // Вывод трейдов
        {

            IExchangeAPI api = null;

            if (exchange.ToLower() == "binance")
            {

                api = ExchangeAPI.GetExchangeAPI<ExchangeBinanceAPI>();
            }

            else if (exchange.ToLower() == "kraken")
            {

                api = ExchangeAPI.GetExchangeAPI<ExchangeKrakenAPI>();
            }

            string mSymbol = "";

            try
            {

                mSymbol = await api.GlobalMarketSymbolToExchangeMarketSymbolAsync(gSymbol); // Получение символа для выбранного API
            }
            catch { }

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
                            chatId,
                            botMsgTradesId,
                            $"{exchange} - {gSymbol}: Amount - {m.Value.Amount}; Price - {m.Value.Price}; Time - {m.Value.Timestamp.ToLocalTime()}");
                    }
                    catch
                    {

                        botMsgTradesId = (await bot.SendTextMessageAsync
                        (
                            chatId,
                            $"{exchange} - {gSymbol}: Amount - {m.Value.Amount}; Price - {m.Value.Price}; Time - {m.Value.Timestamp.ToLocalTime()}")).MessageId;
                    }
                }, mSymbol);

            }

            catch (Exception e)
            {

                Console.WriteLine(e.Message.ToString());
            }

        }

        private async void showCandles(string exchange, string gSymbol, long chatId) // Вывод свечей
        {

            IExchangeAPI api = null;

            if (exchange.ToLower() == "binance")
            {

                api = ExchangeAPI.GetExchangeAPI<ExchangeBinanceAPI>();
            }

            else if (exchange.ToLower() == "kraken")
            {

                api = ExchangeAPI.GetExchangeAPI<ExchangeKrakenAPI>();
            }

            string mSymbol = "";

            try
            {

                mSymbol = await api.GlobalMarketSymbolToExchangeMarketSymbolAsync(gSymbol); // Получение символа для выбранного API
            }
            catch (Exception eMSymbol)
            {

                Console.WriteLine(eMSymbol.Message.ToString());
            }

            botMsgCandlesId = 0;

            try
            {
                if (trades != null)
                    trades.Dispose();

                timerCandles.Elapsed += new ElapsedEventHandler(async (sender, e) =>
                {

                    try
                    {

                        var candles = await api.GetCandlesAsync(mSymbol, 60, CryptoUtility.UtcNow.AddMinutes(-1));
                        try
                        {
                            await bot.EditMessageTextAsync
                            (
                                chatId,
                                botMsgCandlesId,
                                $"Candle {gSymbol} {exchange} Price: {candles.LastOrDefault().OpenPrice};" +
                                $"Base Volume: {candles.LastOrDefault().BaseCurrencyVolume}," +
                                $"Quote volume: {candles.LastOrDefault().QuoteCurrencyVolume}");
                        }
                        catch (Exception)
                        {

                            botMsgCandlesId = (await bot.SendTextMessageAsync
                            (
                                chatId,
                                $"Candle {gSymbol} {exchange} Price: {candles.LastOrDefault().OpenPrice};" +
                                $"Base Volume: {candles.LastOrDefault().BaseCurrencyVolume}," +
                                $"Quote volume: {candles.LastOrDefault().QuoteCurrencyVolume}")).MessageId;
                            timerCandles.Interval = 60000;
                        }

                    }
                    catch (Exception eCandles)
                    {

                        Console.WriteLine(eCandles.Message.ToString());
                    }

                });

                timerCandles.Start();
            }

            catch (Exception eTimer)
            {

                Console.WriteLine(eTimer.Message.ToString());
            }

        }

    }

}
