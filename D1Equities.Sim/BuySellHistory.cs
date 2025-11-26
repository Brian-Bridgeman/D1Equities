namespace D1Equities.Sim
{
    public enum MarketAction
    {
        Buy,
        Sell
    }

    public class BuySellHistory(string symbol, MarketAction action, int quantity, decimal price)
    {
        public string Symbol { get; set; } = symbol;
        public MarketAction Action { get; set; } = action;
        public int Quantity { get; set; } = quantity;
        public decimal Price { get; set; } = price;
        public DateTime Time { get; set; } = DateTime.Now;
        public decimal Total => Quantity * Price;
    }
}