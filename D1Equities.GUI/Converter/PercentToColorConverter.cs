using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace D1Equities.GUI.Converter
{
    public class PercentToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d = 0;
            if (value is double db) d = db;
            else if (value is decimal dec) d = (double)dec;

            if (d > 0) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00895d"));
            if (d < 0) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D50000"));
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
