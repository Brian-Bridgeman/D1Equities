using D1Equities.GUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace D1Equities.GUI.View
{
    /// <summary>
    /// Interaction logic for MarketView.xaml
    /// </summary>
    public partial class MarketView : UserControl
    {
        public MarketView()
        {
            InitializeComponent();
            Loaded += MarketView_Loaded;
        }

        private void MarketView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MarketViewModel vm)
            {
                vm.InitializeAsync();
            }
        }
    }
}
