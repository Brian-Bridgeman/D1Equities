using D1Equities.GUI.View;
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

        private double _currentPrice;

        public double CurrentPrice
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


        public PlotModel CandlestickModel { get; private set; }

        public class StockDetail
        {
            public string Label { get; set; }
            public string Value { get; set; }
        }
        public ObservableCollection<StockDetail> StockDetails { get; } = new ObservableCollection<StockDetail>();
        public StockViewModel()
        {
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
                TrackerFormatString = "Date: {2}\nOpen: {5:0.00000}\nHigh: {3:0.00000}\nLow: {4:0.00000}\nClose: {6:0.00000}",
            };

            var sim = App.Simulator;
            await sim.LoadStock(ticker);
            Ticker = ticker;

            foreach (var candle in sim.SelectedStock.PriceHistory)
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

            CandlestickModel.Series.Add(candleSeries);
            CandlestickModel.InvalidatePlot(true);
            var currentPrice = candleSeries.Items.Last().Close;
            var openPrice = candleSeries.Items.First().Open;
            CurrentPrice = currentPrice;
            PriceDifference = Math.Round((currentPrice - openPrice),2);
            PercentChange = Math.Round(((currentPrice - openPrice) / openPrice) * 100, 2);

        }

    }
}