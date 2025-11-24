using D1Equities.Sim;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace D1Equities.GUI.ViewModel
{
    public class PositionItem : INotifyPropertyChanged
    {
        public string Ticker { get; }
        public int Shares { get; }
        public decimal AveragePrice { get; }

        private decimal _currentPrice;
        public decimal CurrentPrice
        {
            get => _currentPrice;
            set { _currentPrice = value; OnPropertyChanged(); OnPropertyChanged(nameof(ProfitPercent)); }
        }

        public decimal ProfitPercent =>
            AveragePrice == 0 ? 0 : Math.Round(((CurrentPrice - AveragePrice) / AveragePrice) * 100, 2);

        public PositionItem(Position p)
        {
            Ticker = p.Ticker;
            Shares = p.Shares;
            AveragePrice = p.AveragePrice;
            CurrentPrice = p.CurrentPrice;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
