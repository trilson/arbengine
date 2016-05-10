namespace ArbitrageEngine.Model.Results
{
    public interface IArbitrageOpportunity
    {
        decimal TotalBTCTraded { get; }
        decimal TotalUSDTraded { get; }
        decimal ProfitUSD { get; set; }
    }
}