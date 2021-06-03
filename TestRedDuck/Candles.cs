using ExchangeSharp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace TestRedDuck
{
    partial class TGBot
    {

        private int botMsgCandlesId = 0;

        ClientWebSocket webSocket = null;

        private void GetCandles(Telegram.Bot.Types.Message lastMessage) // Вывод биржи для свечей
        {

            if (exchange.ToLower().Contains("binance"))
                GetCandlesBinance(lastMessage);
            else if (exchange.ToLower().Contains("kraken"))
                GetCandlesKraken(lastMessage);
        }

        private async void GetCandlesBinance(Telegram.Bot.Types.Message lastMessage) // Вывод свечей
        {

            await bot.SendTextMessageAsync(lastMessage.Chat.Id, "Result:", replyMarkup: new ReplyKeyboardRemove());

            TakeAPI(lastMessage.Chat.Id);

            TakeSymbol(lastMessage.Chat.Id);

            MarketCandle marketCandle = new MarketCandle();

            string url = $"wss://stream.binance.com:9443/stream?streams={gSymbol.Replace("-", "").ToLower()}@kline_1m";

            try
            {

                await ConnectWebSocketAsync(url, messageCallback: async (_socket, msg) =>
                {
                    JToken token = JToken.Parse(msg.ToStringFromUTF8());
                    string name = token["stream"].ToStringInvariant();
                    token = token["data"];
                    string marketSymbol = mSymbol;

                    marketCandle = api.ParseCandleBinance
                    (
                        token,
                        marketSymbol,
                        60,
                        openKey: "ko",
                        highKey: "kh",
                        lowKey: "kl",
                        closeKey: "kc",
                        timestampKey: "E",
                        timestampType: TimestampType.UnixMilliseconds,
                        baseVolumeKey: "kv",
                        quoteVolumeKey: "kq");

                    try
                    {
                        await bot.EditMessageTextAsync
                        (
                            lastMessage.Chat.Id,
                            botMsgCandlesId,
                            $"Candle {gSymbol} {exchange} Price: {marketCandle.OpenPrice};" +
                            $"Base Volume: {marketCandle.BaseCurrencyVolume}," +
                            $"Quote volume: {marketCandle.QuoteCurrencyVolume}");

                    }
                    catch
                    {
                        botMsgCandlesId = (await bot.SendTextMessageAsync
                                    (
                                        lastMessage.Chat.Id,
                                        $"Candle {gSymbol} {exchange} Price: {marketCandle.OpenPrice};" +
                                        $"Base Volume: {marketCandle.BaseCurrencyVolume}," +
                                        $"Quote volume: {marketCandle.QuoteCurrencyVolume}")).MessageId;
                    }
                });

            }
            catch (Exception eCa)
            {

                Console.WriteLine(eCa.Message.ToString());

            }

        }

        private async void GetCandlesKraken(Telegram.Bot.Types.Message lastMessage) // Вывод свечей
        {

            await bot.SendTextMessageAsync(lastMessage.Chat.Id, "Result:", replyMarkup: new ReplyKeyboardRemove());

            TakeAPI(lastMessage.Chat.Id);

            mSymbol = gSymbol.Replace("-", "/");

            List<string> symd = new List<string>();
            symd.Add(mSymbol);

            MarketCandle marketCandle = new MarketCandle();

            string url = "wss://ws.kraken.com";

            try
            {

                await ConnectWebSocketAsync(url, messageCallback: async (_socket, msg) =>
                {

                    JToken token = JToken.Parse(msg.ToStringFromUTF8());
                    marketCandle = api.ParseCandleKraken
                    (
                        token,
                        mSymbol,
                        60,
                        openKey: 2,
                        highKey: 3,
                        lowKey: 4,
                        closeKey: 5,
                        timestampKey: 0,
                        timestampType: TimestampType.UnixSeconds,
                        baseVolumeKey: 7);

                    try
                    {
                        await bot.EditMessageTextAsync
                        (
                            lastMessage.Chat.Id,
                            botMsgCandlesId,
                            $"Candle {gSymbol} {exchange} Price: {marketCandle.OpenPrice}; " +
                            $"Accumulated volume within interval: {marketCandle.BaseCurrencyVolume}");
                    }
                    catch
                    {
                        botMsgCandlesId = (await bot.SendTextMessageAsync
                                    (
                                        lastMessage.Chat.Id,
                                        $"Candle {gSymbol} {exchange} Price: {marketCandle.OpenPrice}; " +
                                        $"Accumulated volume within interval: {marketCandle.BaseCurrencyVolume}")).MessageId;
                    }
                }, connectCallback: async (_socket) =>
                {
                    await _socket.SendMessageAsync(new
                    {
                        @event = "subscribe",
                        pair = symd,
                        subscription = new { name = "ohlc" }
                    });

                });

            }
            catch (Exception eCa)
            {

                Console.WriteLine(eCa.Message.ToString());

            }

        }
    }
}
