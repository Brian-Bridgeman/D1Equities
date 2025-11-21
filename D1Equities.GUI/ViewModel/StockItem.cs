using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace D1Equities.GUI.ViewModel
{
    public class StockItem : INotifyPropertyChanged
    {
        private string _ticker;
        public string Ticker
        {
            get => _ticker;
            set { _ticker = value; OnPropertyChanged(); }
        }

        private double _price;
        public double Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); OnPropertyChanged(nameof(Percent)); }
        }

        private double _openPrice;
        public double OpenPrice
        {
            get => _openPrice;
            set { _openPrice = value; OnPropertyChanged(); OnPropertyChanged(nameof(Percent)); }
        }

        public double Percent =>
            Math.Round(((Price - OpenPrice) / OpenPrice) * 100, 2);

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
