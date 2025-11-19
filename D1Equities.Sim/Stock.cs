namespace D1Equities.Sim
{
    public class Stock
    {
        public string Symbol { get; }

        public event EventHandler<CandleUpdatedEventArgs>? CandleUpdated;

        public void OnCandleUpdated(CandleStick candle) =>
            CandleUpdated?.Invoke(this, new CandleUpdatedEventArgs(candle));

        public List<CandleStick> PriceHistory { get; set; } = [];

        public CandleStick? CurrentCandle { get; set; } = null;

        public Stock(string symbol)
        {
            Symbol = symbol;
        }
    }
}
