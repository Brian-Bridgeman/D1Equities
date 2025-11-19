namespace D1Equities.Sim;

public class EquityHistory(DateTime dateTime, decimal equity)
{
    public DateTime DateTime { get; } = dateTime;
    public decimal Equity { get; } = equity;
}
