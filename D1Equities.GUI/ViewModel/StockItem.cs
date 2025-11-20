using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D1Equities.GUI.ViewModel
{
    public class StockItem : ViewModelBase
    {
        public string Ticker { get; set; }
        public double Price { get; set; }
        public double OpenPrice { get; set; }
        public double Percent { get; set; }
        public double PriceDifference { get; set; }
        public string CompanyName {  get; set; }
    }
}
