using ArbitrageEngine.Model;
using ArbitrageEngine.Model.Results;
using ArbitrageEngine.Model.Exchange;
using System.Linq;
using System.Collections.Generic;
using System;

namespace ArbitrageEngine
{
    internal class ScenarioCalculator
    {
        private readonly MarketData _marketData;
        private readonly decimal EPSILON = 0.00000001m;

        internal ScenarioCalculator(MarketData marketData)
        {
            _marketData = marketData;
        }

        /// <summary>
        /// Experimental method for calculating arbitrage opportunities from merged bid and ask orderbooks
        /// </summary>
        /// <param name="bidExchanges">Collection of exchanges from which to construct a merged bid orderbook</param>
        /// <param name="askExchanges">Collection of exchanges from which to construct a merged ask orderbook</param>
        /// <returns>a MultiArtbitrageOpportunity containing details of any potential arbitrage opportunities</returns>
        internal IArbitrageOpportunity CalculateMulti(IEnumerable<ExchangePreference> bidExchanges, IEnumerable<ExchangePreference> askExchanges)
        {
            var multiBidBook = new List<KeyValuePair<OrderDetail, ExchangePreference>>();
            foreach (var ex in bidExchanges)
            {
                var orderBook = _marketData.GetOrderBook(ex.ExchangeDetail);
                if (orderBook.HasBids)
                {
                    multiBidBook.AddRange(orderBook.BidDetails.Select(s => new KeyValuePair<OrderDetail, ExchangePreference>(s, ex)));
                }
            }

            var multiAskBook = new List<KeyValuePair<OrderDetail, ExchangePreference>>();
            foreach (var ex in askExchanges)
            {
                var exDepth = _marketData.GetOrderBook(ex.ExchangeDetail);
                if (exDepth.HasAsks)
                {
                    multiAskBook.AddRange(exDepth.AskDetails.Select(s => new KeyValuePair<OrderDetail, ExchangePreference>(s, ex)));
                }
            }
            
            var sortedMultiBidBook = multiBidBook.OrderByDescending(s => s.Key.Price).ToArray();
            var sortedMultiAskBook = multiAskBook.OrderBy(s => s.Key.Price).ToArray();

            // Track currency remaining at each exchange
            var bidBalancesRemaining = bidExchanges.ToDictionary(s => s.ExchangeDetail, t => t.BTCBalance);
            var askBalancesRemaining = askExchanges.ToDictionary(s => s.ExchangeDetail, t => t.USDBalance);     
                   
            var bidCumulativeQuantities = bidExchanges.ToDictionary(s => s.ExchangeDetail, t => 0m);
            var askCumulativeQuantities = askExchanges.ToDictionary(s => s.ExchangeDetail, t => 0m);

            var bidQtyRemaining = new Dictionary<int, decimal>();
            var askQtyRemaining = new Dictionary<int, decimal>();

            decimal cumulativeProfit = 0m, cumulativeSpendUSD = 0m, cumulativeSpendBTC = 0m;

            var bidPriceLevel = new Dictionary<ExchangeDetail, decimal>();
            var askPriceLevel = new Dictionary<ExchangeDetail, decimal>();

            var statistics = new Dictionary<int, Dictionary<int, MultiStat>>();

            int bidLevelIndex = 0;
            int askLevelIndex = 0;

            while (askLevelIndex < sortedMultiAskBook.Length && bidLevelIndex < sortedMultiBidBook.Length)
            {
                var bidDetails = sortedMultiBidBook[bidLevelIndex];
                var askDetails = sortedMultiAskBook[askLevelIndex];
                var bidExchange = bidDetails.Value.ExchangeDetail;
                var askExchange = askDetails.Value.ExchangeDetail;

                // Apply exchange fees to the order book values
                var askValue = askDetails.Key.Price * (1 + askExchange.TradeFeePct);
                var bidValue = bidDetails.Key.Price * (1 - bidExchange.TradeFeePct);

                // Can we make a profit?
                if (askValue >= bidValue)
                {
                    break;
                }

                // Statistics
                if (!statistics.ContainsKey(bidLevelIndex))
                {
                    statistics[bidLevelIndex] = new Dictionary<int, MultiStat>();
                }

                if (!statistics[bidLevelIndex].ContainsKey(askLevelIndex))
                {
                    statistics[bidLevelIndex][askLevelIndex] = new MultiStat
                    {
                        AskOrderDetail = askDetails.Key,
                        ExchangeAsk = askExchange,
                        BidOrderDetail = bidDetails.Key,
                        ExchangeBid = bidExchange,
                    };
                }
                
                var currentStat = statistics[bidLevelIndex][askLevelIndex];

                // If we have an unlimited balance, set a balance equal to the depth we're at.
                var askUSDRemaining = askBalancesRemaining[askExchange] ?? askDetails.Key.Quantity * askValue;
                var bidBTCRemaining = bidBalancesRemaining[bidExchange] ?? bidDetails.Key.Quantity;
                
                if (askUSDRemaining <= EPSILON || bidBTCRemaining <= EPSILON)
                {
                    if (askUSDRemaining <= EPSILON)
                    {
                        askLevelIndex++;
                    }
                    if (bidBTCRemaining <= EPSILON)
                    {
                        bidLevelIndex++;
                    }
                    continue;
                }

                // Order book quantity checks (we may not have consumed the whole level)
                if (!bidQtyRemaining.ContainsKey(bidLevelIndex))
                {
                    bidQtyRemaining[bidLevelIndex] = bidDetails.Key.Quantity;
                }

                if (!askQtyRemaining.ContainsKey(askLevelIndex))
                {
                    askQtyRemaining[askLevelIndex] = askDetails.Key.Quantity;
                }

                // Are we constrained by our USD/BTC balances at each exchange?
                var maxUSDSpend = Math.Min((askQtyRemaining[askLevelIndex] * askValue), askUSDRemaining);
                var maxBTCSell = Math.Min(bidQtyRemaining[bidLevelIndex], bidBTCRemaining);
                var transactionSizeBTC = Math.Min(maxBTCSell, maxUSDSpend / askValue);
                var spentUSD = transactionSizeBTC * askValue;

                // We don't really need all this, but might be useful...?
                currentStat.SpentUSD = spentUSD;
                currentStat.SoldBTC = transactionSizeBTC;
                currentStat.Profit = (transactionSizeBTC * bidValue) - spentUSD;
                cumulativeProfit += currentStat.Profit;
                cumulativeSpendUSD += spentUSD;
                cumulativeSpendBTC += transactionSizeBTC;
                               
                askQtyRemaining[askLevelIndex] -= transactionSizeBTC;
                bidQtyRemaining[bidLevelIndex] -= transactionSizeBTC;

                bidCumulativeQuantities[bidExchange] += transactionSizeBTC;
                bidPriceLevel[bidExchange] = bidDetails.Key.Price;
                askCumulativeQuantities[askExchange] += transactionSizeBTC;
                askPriceLevel[askExchange] = askDetails.Key.Price;

                if (askBalancesRemaining[askExchange] != null)
                {
                    askBalancesRemaining[askExchange] -= spentUSD;
                }

                if (bidBalancesRemaining[bidExchange] != null)
                {
                    bidBalancesRemaining[bidExchange] -= transactionSizeBTC;
                }

                if (askQtyRemaining[askLevelIndex] <= EPSILON || askBalancesRemaining[askExchange] <= EPSILON)
                {
                    askLevelIndex++;
                }

                if (bidQtyRemaining[bidLevelIndex] <= EPSILON || bidBalancesRemaining[bidExchange] <= EPSILON)
                {
                    bidLevelIndex++;
                }
            }

            return new ArbitrageOpportunity
            {
                ProfitUSD = cumulativeProfit,
                TotalUSDTraded = cumulativeSpendUSD,
                TotalBTCTraded = cumulativeSpendBTC,
                BidCumulativeQuantities = bidCumulativeQuantities,
                AskCumulativeQuantities = askCumulativeQuantities,
                BidPrices = bidPriceLevel,
                AskPrices = askPriceLevel,
                Statistics = statistics
            };
        }

        internal bool TryGetArbitrageOpportunity(ExchangePreference marketAsk, ExchangePreference marketBid, out IArbitrageOpportunity opportunity)
        {
            opportunity = CalculateMulti(new[] { marketBid }, new[] { marketAsk });
            return opportunity.ProfitUSD > 0;
        }
    }
}
