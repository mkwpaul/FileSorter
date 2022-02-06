using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WPF.Common.Converters
{
    [ValueConversion(typeof(object), typeof(Visibility))]
    public abstract class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (ToBoolean.ToBool(value))
                return Visibility.Visible;
            else
                return FalseVisibility;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }

        public abstract Visibility FalseVisibility { get; }
    }

    public class ToVisibilityCollapsed : VisibilityConverter
    {
        public override Visibility FalseVisibility => Visibility.Collapsed;
    }

    public class ToVisibilityHidden : VisibilityConverter
    {
        public override Visibility FalseVisibility => Visibility.Hidden;
    }
}
