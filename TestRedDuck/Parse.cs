using ExchangeSharp;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace TestRedDuck
{

    static class Parse // Парсеры для получения свечей
    {

        internal static void ParseVolumesBinance(this JToken token, object baseVolumeKey, object? quoteVolumeKey, decimal last, out decimal baseCurrencyVolume, out decimal quoteCurrencyVolume)
        {

            if (baseVolumeKey == null)
            {
                if (quoteVolumeKey == null)
                {
                    baseCurrencyVolume = quoteCurrencyVolume = 0m;
                }
                else
                {
                    quoteCurrencyVolume = token[$"{quoteVolumeKey.ToString().First()}"][$"{quoteVolumeKey.ToString().Last()}"].ConvertInvariant<decimal>();
                    baseCurrencyVolume = (last <= 0m ? 0m : quoteCurrencyVolume / last);
                }
            }
            else
            {
                baseCurrencyVolume = (token is JObject jObj
                        ? jObj[$"{baseVolumeKey.ToString().First()}"][$"{baseVolumeKey.ToString().Last()}"]
                        : token[$"{baseVolumeKey.ToString().First()}"][$"{baseVolumeKey.ToString().Last()}"]
                    ).ConvertInvariant<decimal>();
                if (quoteVolumeKey == null)
                {
                    quoteCurrencyVolume = baseCurrencyVolume * last;
                }
                else
                {
                    quoteCurrencyVolume = token[$"{quoteVolumeKey.ToString().First()}"][$"{quoteVolumeKey.ToString().Last()}"].ConvertInvariant<decimal>();
                }
            }
        }

        internal static MarketCandle ParseCandleBinance(this INamed named, JToken token, string marketSymbol, int periodSeconds, object openKey, object highKey, object lowKey,
            object closeKey, object timestampKey, TimestampType timestampType, object baseVolumeKey, object? quoteVolumeKey = null, object? weightedAverageKey = null)
         {
            MarketCandle candle = new MarketCandle
            {
                ClosePrice = token[$"{closeKey.ToString().First()}"][$"{closeKey.ToString().Last()}"].ConvertInvariant<decimal>(),
                ExchangeName = named.Name,
                HighPrice = token[$"{highKey.ToString().First()}"][$"{highKey.ToString().Last()}"].ConvertInvariant<decimal>(),
                LowPrice = token[$"{lowKey.ToString().First()}"][$"{lowKey.ToString().Last()}"].ConvertInvariant<decimal>(),
                Name = marketSymbol,
                OpenPrice = token[$"{openKey.ToString().First()}"][$"{openKey.ToString().Last()}"].ConvertInvariant<decimal>(),
                PeriodSeconds = periodSeconds,
                Timestamp = CryptoUtility.ParseTimestamp(token[$"{timestampKey}"], timestampType)
            };

            token.ParseVolumesBinance(baseVolumeKey, quoteVolumeKey, candle.ClosePrice, out decimal baseVolume, out decimal convertVolume);
            candle.BaseCurrencyVolume = (double)baseVolume;
            candle.QuoteCurrencyVolume = (double)convertVolume;
            if (weightedAverageKey != null)
            {
                candle.WeightedAverage = token[weightedAverageKey].ConvertInvariant<decimal>();
            }
            return candle;
        }

        internal static void ParseVolumesKraken(this JToken token, object baseVolumeKey, object? quoteVolumeKey, decimal last, out decimal baseCurrencyVolume, out decimal quoteCurrencyVolume)
        {

            if (baseVolumeKey == null)
            {
                if (quoteVolumeKey == null)
                {
                    baseCurrencyVolume = quoteCurrencyVolume = 0m;
                }
                else
                {
                    quoteCurrencyVolume = token[1][quoteVolumeKey].ConvertInvariant<decimal>();
                    baseCurrencyVolume = (last <= 0m ? 0m : quoteCurrencyVolume / last);
                }
            }
            else
            {
                baseCurrencyVolume = (token is JObject jObj
                        ? jObj[1][baseVolumeKey]
                        : token[1][baseVolumeKey]
                    ).ConvertInvariant<decimal>();
                if (quoteVolumeKey == null)
                {
                    quoteCurrencyVolume = baseCurrencyVolume * last;
                }
                else
                {
                    quoteCurrencyVolume = token[1][quoteVolumeKey].ConvertInvariant<decimal>();
                }
            }
        }

        internal static MarketCandle ParseCandleKraken(this INamed named, JToken token, string marketSymbol, int periodSeconds, object openKey, object highKey, object lowKey,
            object closeKey, object timestampKey, TimestampType timestampType, object baseVolumeKey, object? quoteVolumeKey = null, object? weightedAverageKey = null)
         {
            MarketCandle candle = new MarketCandle
            {
                ClosePrice = token[1][closeKey].ConvertInvariant<decimal>(),
                ExchangeName = named.Name,
                HighPrice = token[1][highKey].ConvertInvariant<decimal>(),
                LowPrice = token[1][lowKey].ConvertInvariant<decimal>(),
                Name = marketSymbol,
                OpenPrice = token[1][openKey].ConvertInvariant<decimal>(),
                PeriodSeconds = periodSeconds,
                Timestamp = CryptoUtility.ParseTimestamp(token[1][timestampKey], timestampType)
            };

            token.ParseVolumesKraken(baseVolumeKey, quoteVolumeKey, candle.ClosePrice, out decimal baseVolume, out decimal convertVolume);
            candle.BaseCurrencyVolume = (double)baseVolume;
            candle.QuoteCurrencyVolume = (double)convertVolume;
            if (weightedAverageKey != null)
            {
                candle.WeightedAverage = token[1][weightedAverageKey].ConvertInvariant<decimal>();
            }
            return candle;
        }

    }
}
