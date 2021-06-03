using ExchangeSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestRedDuck
{

    partial class TGBot
    {

        private delegate void Messages(Telegram.Bot.Types.Message lastMessage);
        private Dictionary<string, Messages> MsgFromUsers;

        private IExchangeAPI api = null;

        public Task<IWebSocket> ConnectWebSocketAsync 
        (
            string url,
            Func<IWebSocket, byte[], Task> messageCallback,
            WebSocketConnectionDelegate? connectCallback = null,
            WebSocketConnectionDelegate? disconnectCallback = null
        )                                                                   // Подключение к веб-сокету
        {
            if (messageCallback == null)
            {

                throw new ArgumentNullException(nameof(messageCallback));
            }

            webSocket = new ClientWebSocket
            {

                Uri = new Uri(url),
                OnBinaryMessage = messageCallback
            };

            if (connectCallback != null)
            {

                webSocket.Connected += connectCallback;
            }

            if (disconnectCallback != null)
            {

                webSocket.Disconnected += disconnectCallback;
            }

            webSocket.Start();

            return Task.FromResult<IWebSocket>(webSocket);
        }

        private async void TakeAPI(long chatId) // Получение API
        {
            try
            {
                if (exchange.ToLower() == "binance")
                {

                    api = ExchangeAPI.GetExchangeAPI<ExchangeBinanceAPI>();
                }

                else if (exchange.ToLower() == "kraken")
                {

                    api = ExchangeAPI.GetExchangeAPI<ExchangeKrakenAPI>();
                }
            }
            catch (Exception)
            {
                var msgFail = new Exception("Don't choose exchange!!");
                await bot.SendTextMessageAsync(chatId, msgFail.Message.ToString());
            }
        }

        private async void TakeSymbol(long chatId) // Получение пары символов
        {
            try
            {

                mSymbol = await api.GlobalMarketSymbolToExchangeMarketSymbolAsync(gSymbol); // Получение символа для выбранного API
            }
            catch (Exception)
            {
                var msgFail = new Exception("Error in symbols!!");
                await bot.SendTextMessageAsync(chatId, msgFail.Message.ToString());
            }
        }

        private void AllMessages() // Команды для чата
        {

            MsgFromUsers = new Dictionary<string, Messages>
            {

                {"/start", KeyExchange},
                {"binance", KeyPair},
                {"kraken", KeyPair},
                {"usdt-btc", KeyData},
                {"btc-usdt", KeyData},
                {"eth-btc", KeyData},
                {"eur-usd", KeyData},
                {"trades", GetTrades},
                {"candles", GetCandles},
                {"/stop", KeyStop}
            };

        }

    }
}
