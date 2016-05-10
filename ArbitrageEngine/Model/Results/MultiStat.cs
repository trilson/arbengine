using ArbitrageEngine.Model.Exchange;

namespace ArbitrageEngine.Model.Results
{
    public class MultiStat
    {
        public OrderDetail AskOrderDetail { get; set; }
        public OrderDetail BidOrderDetail { get; set; }
        public ExchangeDetail ExchangeBid { get; set; }
        public ExchangeDetail ExchangeAsk { get; set; }
        public decimal SpentUSD { get; set; }
        public decimal SoldBTC { get; set; }
        public decimal Profit { get; set; }
    }
}