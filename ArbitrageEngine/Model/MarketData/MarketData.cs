using ArbitrageEngine.Model.Exchange;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ArbitrageEngine.Model
{
	public class MarketData
	{
		private readonly ConcurrentDictionary<ExchangeDetail, OrderBook> exchangeOrderBooks = new ConcurrentDictionary<ExchangeDetail, OrderBook>();

		public void AddOrderBook(ExchangeDetail exchange, OrderBook orderBook)
		{
			exchangeOrderBooks[exchange] = orderBook;
		}

		public OrderBook GetOrderBook(ExchangeDetail e)
		{
			return exchangeOrderBooks.ContainsKey(e) ? exchangeOrderBooks[e] : null;
		}

		public IDictionary<ExchangeDetail, OrderBook> GetExchangeOrderBooks()
		{
			return exchangeOrderBooks;
		}
	}
}