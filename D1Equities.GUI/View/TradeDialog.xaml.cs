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
using System.Windows.Shapes;

namespace D1Equities.GUI.View
{
    /// <summary>
    /// Interaction logic for TradeDialog.xaml
    /// </summary>
    public partial class TradeDialog : Window
    {
        public TradeDialog()
        {
            InitializeComponent();
        }
        private void WindowDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    DragMove();
                }
                catch
                {
                    // DragMove can throw if window isn't shown/modality issues — swallow safely
                }
            }
        }
    }
}
