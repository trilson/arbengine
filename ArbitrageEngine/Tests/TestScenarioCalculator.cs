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
        /// </summary>
        [Test]
        public void TestCalculateSimple()
        {
            var mktData = new MarketData();

            var askExchange = new ExchangeDetail("test1");
            var bidExchange = new ExchangeDetail("test2");            
            var askExchangePref = new ExchangePreference(askExchange);
            var bidExchangePref = new ExchangePreference(bidExchange);

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

            mktData.AddOrderBook(askExchange, askOB);
            mktData.AddOrderBook(bidExchange, bidOB);

            var scenarioCalculator = new ScenarioCalculator(mktData);

            IArbitrageOpportunity arb;
            bool result = scenarioCalculator.TryGetArbitrageOpportunity(askExchangePref, bidExchangePref, out arb);
            Assert.IsTrue(result);
            Assert.IsNotNull(arb);
            Assert.AreEqual(1000m, arb.ProfitUSD);

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
    }
}
