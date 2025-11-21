namespace D1Equities.Sim
{
    public class Stock
    {
        public string Symbol { get; }

        public event EventHandler<CandleUpdatedEventArgs>? CandleUpdated;
        public event EventHandler<NewCandleEventArgs>? NewCandle;

        public void OnCandleUpdated(CandleStick candle) =>
            CandleUpdated?.Invoke(this, new CandleUpdatedEventArgs(candle));

        public void OnNewCandle(CandleStick candle) => NewCandle?.Invoke(this, new NewCandleEventArgs(candle));

        public List<CandleStick> PriceHistory { get; set; } = [];

        public CandleStick? CurrentCandle { get; set; } = null;

        public double GetCurrentPrice() => (double)CurrentCandle.Close;

        public double GetTodaysOpeningPrice()
        {
            var tradingDay = PriceHistory.Last().DateTime.Date;
            return (double)PriceHistory.Where(c => c.DateTime.Date == tradingDay).First().Open;
        }

        public Stock(string symbol)
        {
            Symbol = symbol;
        }
    }
}
