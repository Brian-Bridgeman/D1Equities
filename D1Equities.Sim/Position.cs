using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace D1Equities.Sim
{
    public class Position  
    {
        //Private fields
        private string _ticker;
        private int _shares;
        private decimal _averageEntryPrice;
        private decimal _currentPrice;

        //Properties med PascalCase
        public string Ticker { 
            get
            {
                return _ticker;
            }
            set
            {
                _ticker = value;
            }
        }
        public int Shares
        {
            get
            {
                return _shares;
            }
            set
            {
                _shares = value;
            }
        }
        public decimal AveragePrice
        {
            get
            {
                return _averageEntryPrice;
            }
            set
            {
                _averageEntryPrice = value;
            }
        }
        public decimal CurrentPrice
        {
            get
            {
                return _currentPrice;
            }
            set
            {
                _currentPrice = value;
            }
        }

        //Beräkna properties 
        [JsonIgnore]
        public decimal TotalCost => _shares * _averageEntryPrice;
        [JsonIgnore]
        public decimal CurentValue => _shares * _currentPrice;
        [JsonIgnore]
        public decimal ProfitLoss => CurentValue - TotalCost;
        [JsonIgnore]
        public decimal ProfitLossPercent 
        {
            get
            {
                if (TotalCost == 0) return 0;
                return ProfitLoss / TotalCost * 100;
            }
        }
        public Position() { }
        //Constructor
        public Position(string ticker, int shares, decimal purchasePrice)
        {
            _ticker = ticker;
            _shares = shares;
            _averageEntryPrice = purchasePrice;
            _currentPrice = purchasePrice;
        }

        //Metoder med PascalCase
        public void AddShares(int quantity, decimal Price)
        {
            //Beräkna ny genomsnittspris
            decimal totalCost = (_averageEntryPrice * _shares) + (Price * quantity);
            _shares += quantity;
            _averageEntryPrice = totalCost / _shares;
        }

        public void RemoveShares(int quantity)
        {
            if (quantity > _shares)
                throw new ArgumentException("Kan inte sälja fler aktier än du äger.");
            _shares -= quantity;
        }
        
    }
}
