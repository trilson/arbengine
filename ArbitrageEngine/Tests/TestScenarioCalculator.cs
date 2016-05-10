using ArbitrageEngine.Model;
using ArbitrageEngine.Model.Exchange;
using ArbitrageEngine.Model.Results;
using NUnit.Framework;
using System.Collections.Generic;

namespace ArbitrageEngine.Tests
{
    [TestFixture]
    public class ScenarioCalculatorTest
    {
        /// <summary>
        /// Tests 2 dummy exchanges with limited market data and no constraints.
        /// TODO Put this into a separate TEST project
        /// </summary>
        [Test]
        public void TestCalculateSimple()
        {
            var askExch = new ExchangeDetail("ask");
            var bidExch = new ExchangeDetail("bid");

            var mktData = GetDefaultMarketData(askExch, bidExch);
            var scenarioCalculator = new ScenarioCalculator(mktData);
            var askExchangePref = new ExchangePreference(askExch);
            var bidExchangePref = new ExchangePreference(bidExch);

            IArbitrageOpportunity arb;
            bool result = scenarioCalculator.TryGetArbitrageOpportunity(askExchangePref, bidExchangePref, out arb);
            Assert.IsTrue(result);
            Assert.IsNotNull(arb);
            Assert.AreEqual(1000m, arb.ProfitUSD);
            Assert.AreEqual(100m, arb.TotalBTCTraded);
            Assert.AreEqual(10000m, arb.TotalUSDTraded);

            // Now set a USD limit
            askExchangePref.USDBalance = 100m;
            IArbitrageOpportunity arb2;
            result = scenarioCalculator.TryGetArbitrageOpportunity(askExchangePref, bidExchangePref, out arb2);
            Assert.IsTrue(result);
            Assert.IsNotNull(arb);
            Assert.AreEqual(10m, arb2.ProfitUSD);
            Assert.AreEqual(1m, arb2.TotalBTCTraded);
            Assert.AreEqual(100m, arb2.TotalUSDTraded);
        }

        [Test]
        public void TestCalculateSimpleWithFee()
        {
            var askExch = new ExchangeDetail("ask");
            askExch.TradeFeePct = 0.05m; // take a 5% cut

            var bidExch = new ExchangeDetail("bid");

            var mktData = GetDefaultMarketData(askExch, bidExch);
            var scenarioCalculator = new ScenarioCalculator(mktData);
            var askExchangePref = new ExchangePreference(askExch);
            var bidExchangePref = new ExchangePreference(bidExch);

            IArbitrageOpportunity arb;
            bool result = scenarioCalculator.TryGetArbitrageOpportunity(askExchangePref, bidExchangePref, out arb);
            Assert.IsTrue(result);
            Assert.IsNotNull(arb);
            Assert.AreEqual(500m, arb.ProfitUSD);
            Assert.AreEqual(100m, arb.TotalBTCTraded);
            Assert.AreEqual(10500m, arb.TotalUSDTraded);
        }

        public MarketData GetDefaultMarketData(ExchangeDetail askExch, ExchangeDetail bidExch)
        {
            var mktData = new MarketData();

            var askOB = new OrderBook();
            var bidOB = new OrderBook();
            var asks = new List<OrderDetail>
            {
                new OrderDetail(100, 100)
            };
            var bids = new List<OrderDetail>
            {
                new OrderDetail(110, 100)
            };

            askOB.SetAsks(asks);
            bidOB.SetBids(bids);

            mktData.AddOrderBook(askExch, askOB);
            mktData.AddOrderBook(bidExch, bidOB);

            return mktData;
        }
    }
}