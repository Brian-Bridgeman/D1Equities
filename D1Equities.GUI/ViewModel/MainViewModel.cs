using D1Equities.GUI.Model;
using D1Equities.GUI.View;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace D1Equities.GUI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        // Fields
        private ViewModelBase _currentChildView;
        private string _caption;
        private IconChar _icon;
        private readonly UserModel _currentUser;
        private string[] _userPositionSymbols;

        // Properties
        public ViewModelBase CurrentChildView
        {
            get => _currentChildView;
            set
            {
                _currentChildView = value;
                OnPropertyChanged(nameof(CurrentChildView));
            }
        }

        public string Caption
        {
            get => _caption;
            set
            {
                _caption = value;
                OnPropertyChanged(nameof(Caption));
            }
        }

        public IconChar Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }

        // Commands
        public ICommand ShowHomeViewCommand { get; }
        public ICommand ShowPortfolioViewCommand { get; }
        public ICommand ShowMarketViewCommand { get; }
        public ICommand ShowStockViewCommand { get; }

        // Constructor
        public MainViewModel(UserModel user)
        {
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            _userPositionSymbols = _currentUser.Portfolio.Positions.Keys.ToArray();

            ShowHomeViewCommand = new ViewModelCommand(ExecuteShowHomeViewCommand);
            ShowPortfolioViewCommand = new ViewModelCommand(ExecuteShowPortfolioCommand);
            ShowMarketViewCommand = new ViewModelCommand(ExecuteShowMarketCommand);
            ShowStockViewCommand = new ViewModelCommand(ExecuteShowStockCommand);


            // Default view
            ExecuteShowHomeViewCommand(null);
        }

        // Methods for commands
        private void ExecuteShowHomeViewCommand(object obj)
        {
            UpdateUserPositions();
            App.Simulator.UnsubscribeAllStocks(_userPositionSymbols);
            var vm = new HomeViewModel();
            vm.InitializeAsync();
            CurrentChildView = vm;
            Caption = "Home";
            Icon = IconChar.Home;
        }

        private void ExecuteShowPortfolioCommand(object obj)
        {
            UpdateUserPositions();
            App.Simulator.UnsubscribeAllStocks(_userPositionSymbols);
            // Always use the injected _currentUser
            CurrentChildView = new PortfolioViewModel(_currentUser);
            Caption = "Portfolio";
            Icon = IconChar.CreditCard;
        }

        private void ExecuteShowMarketCommand(object obj)
        {
            UpdateUserPositions();
            App.Simulator.UnsubscribeAllStocks(_userPositionSymbols);
            CurrentChildView = new MarketViewModel();
            Caption = "Market";
            Icon = IconChar.ChartLine;
        }

        private async void ExecuteShowStockCommand(object obj)
        {
            UpdateUserPositions();
            App.Simulator.UnsubscribeAllStocks(_userPositionSymbols);
            string ticker = obj as string ?? "AAPL";

            var vm = new StockViewModel();
            await vm.InitializeAsync(ticker);

            CurrentChildView = vm;
            Caption = ticker;
            Icon = IconChar.ChartLine;
        }
        private void ExecuteBuyStock(string ticker, decimal price, int quantity)
        {
            var user = (UserModel)Application.Current.Properties["User"];
            user.Portfolio.BuyShares(ticker, price, quantity);

            if (CurrentChildView is PortfolioViewModel vm)
                vm.Refresh();
            UpdateUserPositions();
        }

        private void ExecuteSellStock(string ticker, decimal price, int quantity)
        {
            var user = (UserModel)Application.Current.Properties["User"];
            user.Portfolio.SellShares(ticker, price, quantity);

            if (CurrentChildView is PortfolioViewModel vm)
                vm.Refresh();
            UpdateUserPositions();
        }

        private void UpdateUserPositions()
        {
            _userPositionSymbols = _currentUser.Portfolio.Positions.Keys.ToArray();
        }
    }
}
