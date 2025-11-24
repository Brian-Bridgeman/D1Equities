using D1Equities.GUI.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
