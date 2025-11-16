using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D1Equities.Sim
{
    internal class Position
    {
        //Private fields
        private string _ticker;
        private int _shares;
        private decimal _averagePrice;
        private decimal _currentPrice;

        //Properties med PascalCase
        public string Ticker => _ticker;
        public int Shares => _shares;
        public decimal AveragePrice => _averagePrice;
        public decimal CurrentPrice => _currentPrice;

        //Beräkna properties 
        public decimal TotalCost => _shares * _averagePrice;
        public decimal CurentValue => _shares * _currentPrice;
        public decimal ProfitLoss => CurentValue - TotalCost;
        public decimal ProfitLossPercent 
        {
            get
            {
                if (TotalCost == 0) return 0;
                return ProfitLoss / TotalCost * 100;
            }
        }

        //Constructor
        public Position(string ticker, int shares, decimal purchasePrice)
        {
            _ticker = ticker;
            _shares = shares;
            _averagePrice = purchasePrice;
            _currentPrice = purchasePrice;
        }

        //Metoder med PascalCase
        public void AddShares(int quantity, decimal Price)
        {
            //Beräkna ny genomsnittspris
            decimal totalCost = (_averagePrice * _shares) + (Price * quantity);
            _shares += quantity;
            _averagePrice = totalCost / _shares;
        }

        public void RemoveShares(int quantity)
        {
            if (quantity > _shares)
                throw new ArgumentException("Kan inte sälja fler aktier än du äger.");
            _shares -= quantity;
        }
        public string GetPositionSummary()//Sammanfattning av position
        {
            return $"Ticker: {_ticker}, Shares: {_shares}, Avg Price: {_averagePrice:C}, Current Price: {_currentPrice:C}, P/L: {ProfitLoss:C} ({ProfitLossPercent:F2}%)";
        }
    }
}
