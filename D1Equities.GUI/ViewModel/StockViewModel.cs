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

            // Create PlotModel
            CandlestickModel = new PlotModel
            {
                Title = "",
                TitleColor = OxyColors.White,
                Background = OxyColor.FromAColor(0,OxyColor.FromRgb(0,0,0)),
                PlotAreaBackground = OxyColor.FromRgb(25, 25, 25),
                PlotAreaBorderColor = OxyColor.FromRgb(41,41,41),
            };

            DateTimeAxis timeSPanAxis1 = new DateTimeAxis()
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
            CandlestickModel.Axes.Add(timeSPanAxis1);

            LinearAxis linearAxis1 = new LinearAxis()
            {
                Position = AxisPosition.Right,

                MajorGridlineStyle = LineStyle.Dot,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromRgb(44, 44, 44),
                TicklineColor = OxyColor.FromRgb(82, 82, 82),
                TextColor = OxyColors.White,
                TitleColor = OxyColors.White
            };
            CandlestickModel.Axes.Add(linearAxis1);
        }

        public async Task InitializeAsync()
        {
            CandleStickSeries candleSeries = new CandleStickSeries()
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

            await sim.SelectStock("AAPL");

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
            CandlestickModel.ResetAllAxes();
            CandlestickModel.InvalidatePlot(true);
            CurrentPrice = candleSeries.Items.Last().Close;
        }
    }
}