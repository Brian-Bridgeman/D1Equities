using System;
using System.Windows.Input;

namespace D1Equities.GUI.ViewModel
{
    public class TradeDialogViewModel : ViewModelBase
    {
        private int? _shareAmount;
        private int _sharesOwned;

        public bool IsBuy { get; }

        public decimal CurrentPrice { get; set; }
        public decimal Balance { get; set; }

        public int SharesOwned
        {
            get => _sharesOwned;
            set
            {
                if (_sharesOwned != value)
                {
                    _sharesOwned = value;
                    OnPropertyChanged(nameof(SharesOwned));
                    CommandManager.InvalidateRequerySuggested();
                    OnPropertyChanged(nameof(RemainingBalance));
                }
            }
        }

        public int? ShareAmount
        {
            get => _shareAmount;
            set
            {
                if (_shareAmount != value)
                {
                    _shareAmount = value;
                    OnPropertyChanged(nameof(ShareAmount));
                    OnPropertyChanged(nameof(TotalPrice));
                    OnPropertyChanged(nameof(RemainingBalance));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public decimal TotalPrice => ShareAmount.HasValue ? CurrentPrice * ShareAmount.Value : 0;

public decimal RemainingBalance =>
    IsBuy 
        ? Balance - TotalPrice 
        : Balance + Math.Min(ShareAmount.GetValueOrDefault(), SharesOwned) * CurrentPrice;

        public string Title { get; set; } = "";

        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        private readonly Action<bool> _closeCallback;

        public TradeDialogViewModel(bool isBuy, Action<bool> closeCallback)
        {
            IsBuy = isBuy;
            _closeCallback = closeCallback;

            ConfirmCommand = new ViewModelCommand(ExecuteConfirm, CanExecuteConfirm);
            CancelCommand = new ViewModelCommand(_ => _closeCallback(false));
        }

        public void UpdateSharesOwned(int owned)
        {
            SharesOwned = owned;
        }

        private bool CanExecuteConfirm(object? _)
        {
            if (!ShareAmount.HasValue || ShareAmount.Value <= 0)
                return false;

            if (IsBuy)
                return TotalPrice <= Balance;

            // Sell only allowed if ShareAmount <= SharesOwned
            return ShareAmount.Value <= SharesOwned;
        }

        private void ExecuteConfirm(object? _)
        {
            _closeCallback(true);
        }
    }
}
