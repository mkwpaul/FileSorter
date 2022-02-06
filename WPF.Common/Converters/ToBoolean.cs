using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace WPF.Common.Converters
{
    public class ToBoolean : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ToBool(value);
        }

        public static bool ToBool(object obj)
        {
            return obj switch
            {
                null => false,
                string str => str.Length > 0,
                double d => d != 0,
                IEnumerable enumerable => enumerable.GetEnumerator().MoveNext(),
                _ => true,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
