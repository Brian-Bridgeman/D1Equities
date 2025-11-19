using System;

namespace D1Equities.Sim
{
    public class Position : ITradable
    {
        // Privata fält
        private readonly string _ticker;
        private int _shares;
        private decimal _averagePrice;
        private decimal _currentPrice;
        private readonly decimal _entryPrice; // första inpris för positionen

        // Konstruktor
        public Position(string ticker, int shares, decimal purchasePrice)
        {
            if (string.IsNullOrWhiteSpace(ticker)) throw new ArgumentException("Ticker krävs.", nameof(ticker));
            if (shares < 0) throw new ArgumentOutOfRangeException(nameof(shares));
            if (purchasePrice <= 0m) throw new ArgumentOutOfRangeException(nameof(purchasePrice));

            _ticker = ticker;
            _shares = shares;
            _averagePrice = purchasePrice;
            _currentPrice = purchasePrice;
            _entryPrice = purchasePrice;
        }


        public string Ticker => _ticker;
        public decimal CurrentPrice => _currentPrice;

        // Properties med PascalCase
        public decimal EntryPrice => _entryPrice;
        public int Quantity => _shares;
        public decimal Pnl => ProfitLoss;
        public decimal PercentageChange => EntryPrice == 0m ? 0m : (CurrentPrice - EntryPrice) / EntryPrice * 100m;

        // Beräknar properties
        public decimal AveragePrice => _averagePrice;
        public decimal TotalCost => _shares * _averagePrice;
        public decimal CurrentValue => _shares * _currentPrice;
        public decimal ProfitLoss => CurrentValue - TotalCost;
        public decimal ProfitLossPercent => TotalCost == 0m ? 0m : ProfitLoss / TotalCost * 100m;

        // Uppdaterar priset
        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice <= 0m) throw new ArgumentOutOfRangeException(nameof(newPrice));
            _currentPrice = newPrice;
        }

        // Metoder för att ändra position
        public void AddShares(int quantity, decimal price)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            if (price <= 0m) throw new ArgumentOutOfRangeException(nameof(price));

            var totalCost = (_averagePrice * _shares) + (price * quantity);
            _shares += quantity;
            _averagePrice = _shares > 0 ? totalCost / _shares : 0m;
        }

        public void RemoveShares(int quantity)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            if (quantity > _shares) throw new ArgumentException("Kan inte sälja fler aktier än du äger.", nameof(quantity));
            _shares -= quantity;
        }

        public string GetPositionSummary()
        {
            return $"Ticker: {_ticker}, Shares: {_shares}, Entry: {_entryPrice:C}, Avg Price: {_averagePrice:C}, Current: {_currentPrice:C}, P/L: {ProfitLoss:C} ({ProfitLossPercent:F2}%)";
        }
    }
}
