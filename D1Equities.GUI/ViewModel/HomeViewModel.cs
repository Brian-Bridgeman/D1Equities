using D1Equities.GUI.Model;
using D1Equities.Sim;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace D1Equities.GUI.ViewModel
{
    public class HomeViewModel : ViewModelBase
    {
        private decimal _totalEquity;
        private decimal _totalValueChange;
        private decimal _totalPercentageChange;
        private int _totalStocksOwned;
        private UserModel _user;

        public decimal TotalEquity
        {
            get => _totalEquity;
            set
            {
                _totalEquity = value;
                OnPropertyChanged(nameof(TotalEquity));
            }
        }

        public decimal TotalValueChange
        {
            get => _totalValueChange;
            set
            {
                _totalValueChange = value;
                OnPropertyChanged(nameof(TotalValueChange));
            }
        }

        public decimal TotalPercentageChange
        {
            get => _totalPercentageChange;
            set
            {
                _totalPercentageChange = value;
                OnPropertyChanged(nameof(TotalPercentageChange));
            }
        }

        public int TotalStocksOwned
        {
            get => _totalStocksOwned;
            set
            {
                _totalStocksOwned = value;
                OnPropertyChanged(nameof(TotalStocksOwned));
            }
        }


        public HomeViewModel()
        {
            if (Application.Current.Properties.Contains("User") &&
                Application.Current.Properties["User"] is UserModel user &&
                user.Portfolio != null)
            {
                TotalEquity = user.Portfolio.TotalEquity;
                TotalValueChange = user.Portfolio.GetTotalPortfolioValueChange();
                TotalStocksOwned = user.Portfolio.Positions.Count;
                _user = user;
            }
            else
            {
                throw new Exception("Error getting data for signed in user");
            }
        }

        public void InitializeAsync()
        {
            var sim = App.Simulator;
            var positionSymbols = _user.Portfolio.Positions.Keys.ToArray();

            foreach(var pos in _user.Portfolio!.Positions.Values)
            {
                if (sim.IsStockLoaded(pos.Ticker))
                {
                    var stock = sim.GetLoadedStock(pos.Ticker);

                    WeakEventManager<Stock, CandleUpdatedEventArgs>
                        .AddHandler(stock, nameof(stock.CandleUpdated), OnCandleUpdated);

                    WeakEventManager<Stock, NewCandleEventArgs>
                        .AddHandler(stock, nameof(stock.NewCandle), OnNewCandle);
                }
            }
        }

        private void OnCandleUpdated(object? sender, CandleUpdatedEventArgs e)
        {
            UpdatePortfolio(e.Candle.Symbol, e.Candle.Close);
        }

        private void OnNewCandle(object? sender, NewCandleEventArgs e)
        {
            UpdatePortfolio(e.Candle.Symbol, e.Candle.Close);
        }

        private void UpdatePortfolio(string symbol, decimal close)
        {
            if (_user.Portfolio!.Positions.TryGetValue(symbol, out var pos))
                pos.CurrentPrice = close;

            TotalEquity = _user.Portfolio.TotalEquity;
            TotalValueChange = _user.Portfolio.GetTotalPortfolioValueChange();
        }
    }
}
