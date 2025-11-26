using D1Equities.Sim;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace D1Equities.GUI.ViewModel
{
    public class PositionItem : INotifyPropertyChanged
    {
        public string Ticker { get; }
        public int Shares { get; }
        public decimal AveragePrice { get; }
        private Brush _priceFlashColor = Brushes.White;
        public Brush PriceFlashColor
        {
            get => _priceFlashColor;
            set
            {
                _priceFlashColor = value;
                OnPropertyChanged();
            }
        }

        private decimal _currentPrice;
        public decimal CurrentPrice
        {
            get => _currentPrice;
            set
            {
                if (_currentPrice != value)
                {
                    var old = _currentPrice;
                    _currentPrice = value;
                    OnPropertyChanged();

                    if (old != 0)
                    {
                        if (value > old)
                            _ = FlashPriceColor(true);
                        else if (value < old)
                            _ = FlashPriceColor(false);
                    }
                }
                OnPropertyChanged(nameof(ProfitPercent)); }
        }
        private async Task FlashPriceColor(bool wentUp)
        {
            // SET FLASH COLOR (must be on UI thread)
            Application.Current.Dispatcher.Invoke(() =>
            {
                PriceFlashColor = new SolidColorBrush(wentUp ? Color.FromRgb(0, 137, 93) : Colors.Red);

            });

            await Task.Delay(300);

            // RESET (must also be on UI thread)
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                PriceFlashColor = Brushes.White;
            });
        }
        public decimal ProfitPercent => Shares * CurrentPrice - Shares * AveragePrice;

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
