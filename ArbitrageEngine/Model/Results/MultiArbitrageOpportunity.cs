using System.Collections.Generic;
using ArbitrageEngine.Model.Exchange;

namespace ArbitrageEngine.Model.Results
{
    public class ArbitrageOpportunity : IArbitrageOpportunity
    {
        public decimal ProfitUSD { get; set; }
        public decimal TotalUSDTraded { get; set; }
        public decimal TotalBTCTraded { get; set; }

        public Dictionary<ExchangeDetail, decimal> AskCumulativeQuantities { get; set; }
        public Dictionary<ExchangeDetail, decimal> BidCumulativeQuantities { get; set; }

        public Dictionary<ExchangeDetail, decimal> AskPrices { get; set; }
        public Dictionary<ExchangeDetail, decimal> BidPrices { get; set; }

        public Dictionary<int, Dictionary<int, MultiStat>> Statistics { get; set; }
    }
}
