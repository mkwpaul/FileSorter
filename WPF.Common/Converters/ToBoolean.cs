using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace WPF.Common.Converters
{
    public class ToBoolean : IValueConverter, IHasStaticInstance<ToBoolean>
    {
        public static ToBoolean Instance { get; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToBool();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
