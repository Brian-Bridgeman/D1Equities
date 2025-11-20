using D1Equities.GUI.View;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace D1Equities.GUI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        //Fields
        private ViewModelBase _currentChildView;
        private string _caption;
        private IconChar _icon;

        //properties
        public ViewModelBase CurrentChildView
        {
            get
            {
                return _currentChildView;
            }
            set
            {
                _currentChildView = value;
                OnPropertyChanged(nameof(CurrentChildView));
            }
        }
        public string Caption
        {
            get
            {
                return _caption;
            }
            set
            {
                _caption = value;
                OnPropertyChanged(nameof(Caption));
            }
        }
        public IconChar Icon
        {
            get
            {
                return _icon;
            }
            set
            {
                _icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }
        //--> Commands
        public ICommand ShowHomeViewCommand { get; }
        public ICommand ShowPortfolioViewCommand { get; }
        public ICommand ShowMarketViewCommand { get; }
        public ICommand ShowStockViewCommand { get; }

        public MainViewModel()
        {
            //initialize commands
            ShowHomeViewCommand = new ViewModelCommand(ExecuteShowHomeViewCommand);
            ShowPortfolioViewCommand = new ViewModelCommand(ExecuteShowPortfolioCommand);
            ShowMarketViewCommand = new ViewModelCommand(ExecuteShowMarketCommand);
            ShowStockViewCommand = new ViewModelCommand(ExecuteShowStockCommand);


            //default view
            ExecuteShowHomeViewCommand(null);
        }

        private void ExecuteShowHomeViewCommand(object obj)
        {
            CurrentChildView = new HomeViewModel();
            Caption = "Home";
            Icon = IconChar.Home;
        }
        private void ExecuteShowPortfolioCommand(object obj)
        {
            CurrentChildView = new PortfolioViewModel();
            Caption = "Portfolio";
            Icon = IconChar.CreditCard;
        }

        private void ExecuteShowMarketCommand(object obj)
        {
            CurrentChildView = new MarketViewModel();
            Caption = "Market";
            Icon = IconChar.ChartLine;

        }
        private async void ExecuteShowStockCommand(object obj)
        {
            string ticker = obj as string ?? "AAPL";

            var vm = new StockViewModel();
            await vm.InitializeAsync(ticker);



            CurrentChildView = vm; // assign the view, not the VM
            Caption = ticker;
            Icon = IconChar.ChartLine;

        }
    }
}
