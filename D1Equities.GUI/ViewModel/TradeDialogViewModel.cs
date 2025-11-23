using System;
using System.Windows.Input;

namespace D1Equities.GUI.ViewModel
{
    public class TradeDialogViewModel : ViewModelBase
    {
        public decimal CurrentPrice { get; set; }
        public int ShareAmount { get; set; }
        public string Title { get; set; } = "";

        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public TradeDialogViewModel(Action<bool> closeCallback)
        {
            ConfirmCommand = new RelayCommand(_ => closeCallback(true));
            CancelCommand = new RelayCommand(_ => closeCallback(false));
        }
    }
}