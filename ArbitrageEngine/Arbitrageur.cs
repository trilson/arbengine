using System.Collections.Generic;
using ArbitrageEngine.Model;
using ArbitrageEngine.Model.Exchange;
using ArbitrageEngine.Model.Results;
using System.Linq;

namespace ArbitrageEngine
{
	public static class Arbitrageur
	{
		public static IEnumerable<IArbitrageOpportunity> Calculate(IEnumerable<ExchangePreference> exchangePrefs, MarketData marketData)
		{
			var scenarioCalculator = new ScenarioCalculator(marketData);
            foreach (var askMarket in exchangePrefs)
            {
                var allOthers = exchangePrefs.Where(e => e != askMarket);
                foreach (var bidMarket in allOthers)
                {
                    IArbitrageOpportunity opp;
                    if (scenarioCalculator.TryGetArbitrageOpportunity(askMarket, bidMarket, out opp))
                    {
                        yield return opp;
                    }
                }
            }
		}
        
		public static bool TryCalculate(ExchangePreference epBid, ExchangePreference epAsk, MarketData marketData, out IArbitrageOpportunity arbOpp)
		{
			var scenarioCalculator = new ScenarioCalculator(marketData);
			return scenarioCalculator.TryGetArbitrageOpportunity(epBid, epAsk, out arbOpp);
		}

		public static IEnumerable<IArbitrageOpportunity> CalculateMulti(MarketData marketData, IEnumerable<ExchangePreference> exchangePrefs)
		{
			var scenarioCalculator = new ScenarioCalculator(marketData);
			yield return scenarioCalculator.CalculateMulti(exchangePrefs, exchangePrefs);

			foreach (ExchangePreference exchangePref in exchangePrefs) 
			{
				yield return scenarioCalculator.CalculateMulti(new[] { exchangePref }, exchangePrefs);
				yield return scenarioCalculator.CalculateMulti(exchangePrefs, new[] { exchangePref });
			}				
		}
	}
}