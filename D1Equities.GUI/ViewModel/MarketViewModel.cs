using D1Equities.Sim;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;

namespace D1Equities.GUI.ViewModel
{
    public class MarketViewModel : ViewModelBase
    {
        private double _currentPrice;
        private readonly CancellationTokenSource? _searchCts;
        private readonly System.Timers.Timer? _debounceTimer;
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

        private string? _searchText;
        public string SearchText
        {
            get => _searchText ?? string.Empty;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterResults();
            }
        }

        private ObservableCollection<Ticker> _stocks = new();
        public ObservableCollection<Ticker> Stocks
        {
            get => _stocks;
            set
            {
                _stocks = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<MostActiveStock> MostActive { get; } = new();
        public ObservableCollection<MoverItem> MarketMovers { get; } = new();

        public void InitializeAsync()
        {
            var sim = App.Simulator;

            if(sim.MarketMovers != null)
            {
                foreach(var mover in sim.MarketMovers.Gainers)
                {
                    MarketMovers.Add(mover);
                }
            }

            if(sim.MostActiveStocks != null)
            {
                foreach(var stock in sim.MostActiveStocks.MostActives)
                {
                    MostActive.Add(stock);
                }
            }

            Stocks = new ObservableCollection<Ticker>(sim.AvailableSymbols.Values);
        }

        public async void FilterResults()
        {
            var sim = App.Simulator;
            string query = SearchText;

            var results = await Task.Run(() =>
            {
                var tickers = sim.AvailableSymbols.Values;
                if (string.IsNullOrWhiteSpace(query))
                    return tickers;

                return tickers
                    .Where(t =>
                        t.Symbol.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        t.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
            });

            Stocks = new ObservableCollection<Ticker>(results);
        }
    }
}
