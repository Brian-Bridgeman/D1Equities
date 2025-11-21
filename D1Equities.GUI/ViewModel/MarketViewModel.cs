using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace D1Equities.GUI.ViewModel
{
    public class MarketViewModel : ViewModelBase
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

            foreach(var ticker in sim.AvailableSymbols.Take(9))
            {
                await sim.LoadStock(ticker);
                var stock = sim.TryGetLoadedStock(ticker);
                var currentPrice = stock.GetCurrentPrice();
                var openPrice = stock.GetTodaysOpeningPrice();
                Stocks.Add(new StockItem
                {
                    Ticker = stock.Symbol,
                    Price = currentPrice,
                    OpenPrice = openPrice,
                    Percent = Math.Round(((currentPrice - openPrice) / openPrice) * 100, 2),
                });
            }
        }
    }
}
