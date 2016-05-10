namespace ArbitrageEngine.Model.Exchange
{
	public class ExchangeDetail
	{
		public ExchangeDetail (string name, string description = null)
		{
			Name = name;
			Description = description;
		}

		public string Name { get; private set; }

		public string Description { get; private set; }

		public decimal TradeFeePct { get; set; }
	}
}