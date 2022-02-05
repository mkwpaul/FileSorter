using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace WPF.Common.Converters
{
    public class ByteCountToFormattedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            switch (value)
            {
                case int i: return SizeSuffix(i);
                case long l: return SizeSuffix(l);
                case double d: return SizeSuffix((long)d);
                case decimal n: return SizeSuffix((long)n);
                default: return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        static string SizeSuffix(long value, int decimalPlaces = 3)
        {
            if (decimalPlaces < 0) 
                throw new ArgumentOutOfRangeException(nameof(decimalPlaces));

            if (value < 0) 
                return "-" + SizeSuffix(-value, decimalPlaces);

            if (value == 0) 
                return string.Format("{0:n" + decimalPlaces + "} bytes", 0);

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag++;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
        }
    }
}
