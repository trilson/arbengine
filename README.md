# An experimental USD / BTC arb library

Use with caution. Tests are pending!

* Supports the calculation of arbitrage opportunities given a set of exchanges and market data.
* Commission / fees can be set (optionally) at the exchange level.
* Arbitrage opportunities can be optimised by setting an optional USD and BTC limit at each exchange.
* Multipe exchange orderbooks can be combined to return more complex opportunities (e.g. buy at x, y, z, sell at a, b, c). These should honour any existing exchange preferences (fees / balances)
