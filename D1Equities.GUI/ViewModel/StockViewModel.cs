using D1Equities.GUI.Model;
using D1Equities.GUI.View;
using D1Equities.Sim;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static D1Equities.GUI.ViewModel.StockViewModel;

namespace D1Equities.GUI.ViewModel
{
    public class StockViewModel : ViewModelBase
    {

        private decimal _currentPrice;
        private CandleStickSeries _candleSeries;

        public decimal CurrentPrice
        {
            get => _currentPrice;
            set
            {
                _currentPrice = value;
                OnPropertyChanged(nameof(CurrentPrice));
            }
        }
        private double _openPrice;
        public double OpenPrice
        {
            get => _openPrice;
            set
            {
                _openPrice = value;
                OnPropertyChanged(nameof(OpenPrice));
            }
        }
        private double _priceDifference;
        public double PriceDifference
        {
            get => _priceDifference;
            set
            {
                _priceDifference = value;
                OnPropertyChanged(nameof(PriceDifference));
            }
        }
        private double _percentChange;
        public double PercentChange
        {
            get => _percentChange;
            set
            {
                _percentChange = value;
                OnPropertyChanged(nameof(PercentChange));
            }
        }
        private string _ticker;
        public string Ticker
        {
            get => _ticker;
            set { _ticker = value; OnPropertyChanged(nameof(Ticker)); }
        }

        private string _companyName;
        public string CompanyName
        {
            get => _companyName;
            set { _companyName = value; OnPropertyChanged(nameof(CompanyName)); }
        }

        public ICommand BuyCommand { get; }
        public ICommand SellCommand { get; }


        public PlotModel CandlestickModel { get; private set; }

        public class StockDetail
        {
            public string Label { get; set; }
            public string Value { get; set; }
        }
        public ObservableCollection<StockDetail> StockDetails { get; } = new ObservableCollection<StockDetail>();
        public StockViewModel()
        {
            BuyCommand = new ViewModelCommand(_ => OpenTradeDialog(true));
            SellCommand = new ViewModelCommand (_ => OpenTradeDialog(false));

            StockDetails.Add(new StockDetail { Label = "Market Cap", Value = "2.5T" });
            StockDetails.Add(new StockDetail { Label = "P/E Ratio", Value = "28.7" });
            StockDetails.Add(new StockDetail { Label = "Dividend Yield", Value = "0.55%" });
            StockDetails.Add(new StockDetail { Label = "Open", Value = "$175.36" });
            StockDetails.Add(new StockDetail { Label = "Low", Value = "173.54" });
            StockDetails.Add(new StockDetail { Label = "Volume", Value = "51.2M" });
        }

        public async Task InitializeAsync(string ticker)
        {
            // Create a fresh PlotModel for this stock
            CandlestickModel = new PlotModel
            {
                TitleColor = OxyColors.White,
                Background = OxyColor.FromAColor(0, OxyColor.FromRgb(0, 0, 0)),
                PlotAreaBackground = OxyColor.FromRgb(25, 25, 25),
                PlotAreaBorderColor = OxyColor.FromRgb(41, 41, 41),
            };

            var timeAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                MinorIntervalType = DateTimeIntervalType.Auto,
                MajorGridlineStyle = LineStyle.Dot,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromRgb(44, 44, 44),
                TicklineColor = OxyColor.FromRgb(82, 82, 82),
                TextColor = OxyColors.White,
                TitleColor = OxyColors.White
            };
            CandlestickModel.Axes.Add(timeAxis);

            var linearAxis = new LinearAxis
            {
                Position = AxisPosition.Right,
                MajorGridlineStyle = LineStyle.Dot,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromRgb(44, 44, 44),
                TicklineColor = OxyColor.FromRgb(82, 82, 82),
                TextColor = OxyColors.White,
                TitleColor = OxyColors.White
            };
            CandlestickModel.Axes.Add(linearAxis);

            // Fill the candlestick series
            var candleSeries = new CandleStickSeries
            {
                Color = OxyColors.Black,
                IncreasingColor = OxyColor.FromRgb(0, 137, 93),
                DecreasingColor = OxyColor.FromRgb(163, 0, 0),
                DataFieldX = "Time",
                DataFieldHigh = "H",
                DataFieldLow = "L",
                DataFieldClose = "C",
                DataFieldOpen = "O",
                CandleWidth = 0.0005,
                TrackerFormatString = "Date: {2}\nOpen: {5:0.00000}\nHigh: {3:0.00000}\nLow: {4:0.00000}\nClose: {6:0.00000}",
            };

            var sim = App.Simulator;
            Ticker = ticker;
            CompanyName = sim.AvailableSymbols[Ticker].Name;

            if (!sim.IsStockLoaded(ticker))
                await sim.LoadStock(ticker);

            if (!sim.IsStockLoaded(ticker))
                return;

            var stock = sim.GetLoadedStock(ticker);

            foreach (var candle in stock.PriceHistory)
            {
                candleSeries.Items.Add(
                    new HighLowItem(
                        DateTimeAxis.ToDouble(candle.DateTime),
                        (double)candle.High,
                        (double)candle.Low,
                        (double)candle.Open,
                        (double)candle.Close
                    )
                );
            }

            _candleSeries = candleSeries;

            stock.CandleUpdated += Stock_CandleUpdated;
            stock.NewCandle += Stock_NewCandle;

            CandlestickModel.Series.Add(candleSeries);
            CandlestickModel.InvalidatePlot(true);
            var currentPrice = candleSeries.Items.Last().Close;
            var openPrice = candleSeries.Items.First().Open;
            CurrentPrice = (decimal)currentPrice;
            PriceDifference = Math.Round((currentPrice - openPrice),2);
            PercentChange = Math.Round(((currentPrice - openPrice) / openPrice) * 100, 2);
        }

        private void Stock_NewCandle(object? sender, NewCandleEventArgs e)
        {
            var item = new HighLowItem(
                DateTimeAxis.ToDouble(e.Candle.DateTime),
                (double)e.Candle.High,
                (double)e.Candle.Low,
                (double)e.Candle.Open,
                (double)e.Candle.Close);

            _candleSeries.Items.Add(item);
            CandlestickModel.InvalidatePlot(false);
        }

        private void Stock_CandleUpdated(object? sender, CandleUpdatedEventArgs e)
        {
            int idx = _candleSeries.Items.Count - 1;
            var last = _candleSeries.Items[idx];

            last.Open = (double)e.Candle.Open;
            last.Close = (double)e.Candle.Close;
            last.High = (double)e.Candle.High;
            last.Low = (double)e.Candle.Low;

            _candleSeries.Items[idx] = last;
            CandlestickModel.InvalidatePlot(false);
        }
        private void OpenTradeDialog(bool isBuy)
        {
            TradeDialog? dialog = null;
            TradeDialogViewModel? vm = null;

            vm = new TradeDialogViewModel(isBuy,result =>
            {
                if (result)
                {
                    ExecuteTrade(isBuy, vm!.ShareAmount.GetValueOrDefault(), vm!.CurrentPrice);
                }

                dialog?.Close();
            });

            var user = Application.Current.Properties["User"] as UserModel;

            vm.CurrentPrice = this.CurrentPrice;
            vm.Balance = user.Portfolio.Balance;      
            vm.Title = isBuy ? "Buy Shares" : "Sell Shares";
            int owned = user?.Portfolio.Positions.TryGetValue(Ticker, out var pos) == true ? pos.Shares : 0;
            vm.UpdateSharesOwned(owned);

            dialog = new TradeDialog { DataContext = vm };
            dialog.ShowDialog();
        }




        private void ExecuteTrade(bool isBuy, int amount, decimal price)
        {
            if (amount <= 0) return;

            var user = Application.Current.Properties["User"] as UserModel;



            if (isBuy)
            {
                try
                {
                    user.Portfolio.OpenPosition(Ticker, price, amount);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Buy Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                try
                {
                    user.Portfolio.ClosePosition(Ticker);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Sell Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            user.Portfolio.Save(); // persist portfolio changes
        }
    }
}