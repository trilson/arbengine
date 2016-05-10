namespace ArbitrageEngine.Model.Exchange
{
	public class ExchangePreference
    { 
        public ExchangePreference(ExchangeDetail exchange)
        {
            ExchangeDetail = exchange;
        }

        public ExchangeDetail ExchangeDetail { get; private set; }

		public decimal? BTCBalance { get; set; }

		public decimal? USDBalance { get; set; }
	}
}