using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D1Equities.GUI.ViewModel
{
    public static class PortfolioService
    {
        public static void BuyStock(string ticker, int amount, decimal price)
        {
            // TODO: implement real portfolio logic
            Console.WriteLine($"Bought {amount} shares of {ticker} at {price}");
        }

        public static void SellStock(string ticker, int amount, decimal price)
        {
            // TODO: implement real portfolio logic
            Console.WriteLine($"Sold {amount} shares of {ticker} at {price}");
        }
    }

}
