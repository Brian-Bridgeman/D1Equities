using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D1Equities.GUI.ViewModel
{
    public class MarketViewModel:ViewModelBase
    {
        private double _currentPrice;
        public double CurrentPrice
        {
            get => _currentPrice;
            set
            {
                _currentPrice = value;
                OnPropertyChanged(nameof(CurrentPrice));
            }
        }
        private double _openPrice;
        public double OpenPrice
        {
            get => _openPrice;
            set
            {
                _openPrice = value;
                OnPropertyChanged(nameof(OpenPrice));
            }
        }

        public ObservableCollection<StockItem> Stocks { get; } = new();

        public async Task InitializeAsync()
        {
            var sim = App.Simulator;
            

            await sim.SelectStock("AAPL");
            var currentPrice = (double)sim.SelectedStock.PriceHistory.Last().Close;
            var openPrice = (double)sim.SelectedStock.PriceHistory.First().Close;
            Stocks.Add(new StockItem
            {
                Ticker = "AAPL",
                Price = currentPrice,
                OpenPrice = openPrice,
                Percent = Math.Round(((currentPrice - openPrice)/openPrice) * 100, 2),
            });

            await sim.SelectStock("MSFT");
             currentPrice = (double)sim.SelectedStock.PriceHistory.Last().Close;
             openPrice = (double)sim.SelectedStock.PriceHistory.First().Close;
            Stocks.Add(new StockItem
            {
                Ticker = "MSFT",
                Price = currentPrice,
                OpenPrice = openPrice,
                Percent = Math.Round(((currentPrice - openPrice) / openPrice) * 100, 2),
            });
            await sim.SelectStock("TSLA");
            currentPrice = (double)sim.SelectedStock.PriceHistory.Last().Close;
            openPrice = (double)sim.SelectedStock.PriceHistory.First().Close;
            Stocks.Add(new StockItem
            {
                Ticker = "TSLA",
                Price = currentPrice,
                OpenPrice = openPrice,
                Percent = Math.Round(((currentPrice - openPrice) / openPrice) * 100, 2),
            });
        }
    }
}
