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
            StockDetails.Add(new StockDetail { Label = "Dividend Yield", Value = "51.2M" });

            // Create PlotModel
            CandlestickModel = new PlotModel
            {
                Title = "",
                TitleColor = OxyColors.White,
                Background = OxyColor.FromAColor(0,OxyColor.FromRgb(0,0,0)),
                PlotAreaBackground = OxyColor.FromRgb(25, 25, 25),
                PlotAreaBorderColor = OxyColors.White,
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

            CandleStickSeries candle = new CandleStickSeries()
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


            var random = new Random();
            DateTime start = DateTime.Now.AddDays(-100); // 100 days of data
            double lastClose = 100;

            for (int i = 0; i < 100; i++)
            {
                double open = lastClose + random.NextDouble() * 4 - 2; 
                double high = open + random.NextDouble() * 5;
                double low = open - random.NextDouble() * 5;
                double close = low + random.NextDouble() * (high - low);

                candle.Items.Add(new HighLowItem(DateTimeAxis.ToDouble(start.AddDays(i)), high, low, open, close));
                lastClose = close;
            }

            CandlestickModel.Series.Add(candle);

            // Zoom out so all data is visible by default
            CandlestickModel.ResetAllAxes();
        }

    }
}