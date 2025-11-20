using D1Equities.GUI.Model;
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
            }
            else
            {
                // fallback defaults
                TotalEquity = 0;
                TotalValueChange = 0;
                TotalStocksOwned = 0;
            }
        }
    }
}
