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

        public async Task InitializeAsync()
        {
            var sim = App.Simulator;

            await sim.UnloadAllStocks();

            foreach(var pos in _user.Portfolio!.Positions.Values)
            {
                await sim.LoadStock(pos.Ticker);

                if (sim.IsStockLoaded(pos.Ticker))
                {
                    var stock = sim.GetLoadedStock(pos.Ticker);

                    stock.CandleUpdated += Stock_CandleUpdated;
                }

            }
        }

        private void Stock_CandleUpdated(object? sender, CandleUpdatedEventArgs e)
        {
            if(_user.Portfolio!.Positions.TryGetValue(e.Candle.Symbol, out var pos))
            {
                pos.CurrentPrice = e.Candle.Close;
            }

            TotalEquity = _user.Portfolio.TotalEquity;
            TotalValueChange = _user.Portfolio.GetTotalPortfolioValueChange();
        }
    }
}
