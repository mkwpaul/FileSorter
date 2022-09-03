﻿using System.Globalization;
using System.Windows.Data;

namespace WPF.Common.Converters
{
    public class IsNotNull : IValueConverter, IHasStaticInstance<IsNotNull>
    {
        public static IsNotNull Instance { get; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is not null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
