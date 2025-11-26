using D1Equities.GUI.Model;
using D1Equities.Sim;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace D1Equities.GUI.ViewModel
{
    public class PortfolioViewModel : ViewModelBase
    {
        public ObservableCollection<PositionItem> Positions { get; }
        private UserModel _user;
        public PortfolioViewModel(UserModel user)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));

            Positions = new ObservableCollection<PositionItem>(
                _user.Portfolio.Positions.Values.Select(p => new PositionItem(p))
            );

            var sim = App.Simulator;

            foreach(var pos in Positions)
            {
                if (!sim.IsStockLoaded(pos.Ticker))
                    throw new Exception("position stock should always be loaded");

                var stock = sim.GetLoadedStock(pos.Ticker);


                WeakEventManager<Stock, CandleUpdatedEventArgs>
                    .AddHandler(stock, nameof(stock.CandleUpdated), OnCandleUpdated);

                WeakEventManager<Stock, NewCandleEventArgs>
                    .AddHandler(stock, nameof(stock.NewCandle), OnNewCandle);
            }
        }

        private void OnNewCandle(object? sender, NewCandleEventArgs e)
        {
            UpdatePosition(e.Candle);
        }

        private void OnCandleUpdated(object? sender, CandleUpdatedEventArgs e)
        {
            UpdatePosition(e.Candle);
        }

        private void UpdatePosition(CandleStick c)
        {
            foreach(var pos in Positions)
            {
                if(pos.Ticker == c.Symbol)
                {
                    pos.CurrentPrice = c.Close;
                }
            }
        }

        public void Refresh()
        {
            // Clear and reload positions
            Positions.Clear();
            foreach (var p in _user.Portfolio.Positions.Values)
                Positions.Add(new PositionItem(p));
        }
    }
}
