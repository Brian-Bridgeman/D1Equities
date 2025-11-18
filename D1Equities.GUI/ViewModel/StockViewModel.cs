using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;

namespace D1Equities.GUI.ViewModel
{
    public class StockViewModel : ViewModelBase
    {
        public PlotModel CandlestickModel { get; private set; }

        public StockViewModel()
        {
            // Create PlotModel
            CandlestickModel = new PlotModel
            {
                Title = "",
                TitleColor = OxyColors.White,
                Background = OxyColor.FromRgb(34, 34, 34) // match panel background

            };

            // X-axis (dates)
            var dateAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "MMM dd",
                Title = "Date",
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                AxislineColor = OxyColors.White
            };
            CandlestickModel.Axes.Add(dateAxis);

            // Y-axis (price)
            var priceAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Price",
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.White,
                AxislineColor = OxyColors.White
            };
            CandlestickModel.Axes.Add(priceAxis);

            // Candlestick series
            var candleSeries = new CandleStickSeries
            {
                Color = OxyColors.Red,
                IncreasingColor = OxyColors.Green,
                CandleWidth = 0.6
            };

            // Generate a lot more sample data
            var random = new Random();
            DateTime start = DateTime.Now.AddDays(-100); // 100 days of data
            double lastClose = 100;

            for (int i = 0; i < 100; i++)
            {
                double open = lastClose + random.NextDouble() * 4 - 2; // fluctuate ±2
                double high = open + random.NextDouble() * 5;
                double low = open - random.NextDouble() * 5;
                double close = low + random.NextDouble() * (high - low);

                candleSeries.Items.Add(new HighLowItem(DateTimeAxis.ToDouble(start.AddDays(i)), high, low, open, close));
                lastClose = close;
            }

            CandlestickModel.Series.Add(candleSeries);

            // Zoom out so all data is visible by default
            CandlestickModel.ResetAllAxes();
        }
    }
}