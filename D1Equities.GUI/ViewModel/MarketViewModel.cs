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
                OnPropertyChanged();
                StartDebounce();
            }
        }

        public ObservableCollection<StockItem> Stocks { get; } = new();

        public async Task InitializeAsync()
        {
            var sim = App.Simulator;

            foreach(var ticker in sim.AvailableSymbols.Take(9))
            {
                await sim.LoadStock(ticker);
                var stock = sim.GetLoadedStock(ticker);
                stock.CandleUpdated += Stock_CandleUpdated;

                var currentPrice = stock.GetCurrentPrice();
                var openPrice = stock.GetTodaysOpeningPrice();
                Stocks.Add(new StockItem
                {
                    Ticker = stock.Symbol,
                    Price = currentPrice,
                    OpenPrice = openPrice,
                });
            }
        }

        private void StartDebounce()
        {
            if (_debounceTimer == null)
            {
                _debounceTimer = new System.Timers.Timer(250);
                _debounceTimer.AutoReset = false;
                _debounceTimer.Elapsed += async (_, __) =>
                {
                    int version = ++_searchVersion;
                    await App.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        await FilterResults(version);
                    });
                };
            }

            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        public async Task FilterResults(int version)
        {
            if (string.IsNullOrWhiteSpace(_searchText))
                return;

            var loadedSymbols = App.Simulator.GetAllLoadedSymbols();

            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                var sim = App.Simulator;

                foreach (var ticker in loadedSymbols)
                {
                    var stock = App.Simulator.GetLoadedStock(ticker);
                    stock.CandleUpdated -= Stock_CandleUpdated;
                    App.Simulator.UnloadStock(ticker);
                }

                Stocks.Clear();

                IEnumerable<string> matches;

                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    matches = sim.AvailableSymbols.Take(9);
                }
                else
                {
                    matches = sim.AvailableSymbols
                        .Where(s => s.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                        .Take(9);
                }

                foreach (var ticker in matches)
                {
                    token.ThrowIfCancellationRequested();

                    if (version != _searchVersion)
                        return;

                    await sim.LoadStock(ticker);

                    var stock = sim.GetLoadedStock(ticker);

                    var item = new StockItem
                    {
                        Ticker = ticker,
                        Price = stock.GetCurrentPrice(),
                        OpenPrice = stock.GetTodaysOpeningPrice()
                    };

                    Stocks.Add(item);

                    stock.CandleUpdated += Stock_CandleUpdated;
                }
            }
            catch (OperationCanceledException) {}
        }


        private void Stock_CandleUpdated(object sender, CandleUpdatedEventArgs e)
        {
            var item = Stocks.FirstOrDefault(s => s.Ticker == e.Candle.Symbol);

            if (item != null)
                item.Price = (double)e.Candle.Close;
        }
    }
}
