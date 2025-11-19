using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D1Equities.Sim
{
    public interface ITradable
    {
        string Ticker { get; }
        decimal CurrentPrice { get; }
        void UpdatePrice(decimal newPrice);
    }


    public abstract class TradableBase : ITradable
    {
        
        protected TradableBase(string ticker, decimal initialPrice)
        {
            Ticker = ticker ?? throw new ArgumentNullException(nameof(ticker));
            if (initialPrice <= 0m) throw new ArgumentOutOfRangeException(nameof(initialPrice));
            CurrentPrice = initialPrice;
        }

        public string Ticker { get; }

        public decimal CurrentPrice { get; private set; }

        public virtual void UpdatePrice(decimal newPrice)
        {
            if (newPrice <= 0m) throw new ArgumentOutOfRangeException(nameof(newPrice));
            CurrentPrice = newPrice;
        }
    }
}
