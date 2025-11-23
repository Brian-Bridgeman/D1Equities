using D1Equities.Sim;
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
        private CancellationTokenSource _searchCts;
        private System.Timers.Timer _debounceTimer;
        private int _searchVersion = 0;

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

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                FilterResults();
                OnPropertyChanged();
            }
        }

        public ObservableCollection<StockItem> Stocks { get; } = new();

        public void InitializeAsync()
        {
            var sim = App.Simulator;

            foreach(var ticker in sim.AvailableSymbols)
            {
                Stocks.Add(new StockItem
                {
                    Ticker = ticker,
                });
            }
        }

        public void FilterResults()
        {
            var sim = App.Simulator;

            Stocks.Clear();

            IEnumerable<string> matches;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                matches = sim.AvailableSymbols;
            }
            else
            {
                matches = sim.AvailableSymbols
                    .Where(s => s.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .Take(9);
            }

            foreach (var ticker in matches)
            {
                var item = new StockItem
                {
                    Ticker = ticker,
                };

                Stocks.Add(item);

            }
        }
    }
}
