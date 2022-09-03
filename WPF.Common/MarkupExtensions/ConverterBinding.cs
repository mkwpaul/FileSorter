using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using WPF.Common.Converters;

namespace WPF.Common.MarkupExtensions;

[DefaultProperty(nameof(Path))]
public abstract class ConverterBinding<T> : CustomBindingBase where T : IValueConverter, IHasStaticInstance<T>
{
    private converter? _converter;

    protected ConverterBinding()
    {
        Binding.Converter = T.Instance;
    }

    protected ConverterBinding(string path) : base(path)
    {
        Binding.Converter = T.Instance;
    }

    public override IValueConverter? Converter
    {
        get => _converter?.UserConverter;
        set
        {
            if (_converter is null)
                Binding.Converter = _converter = new();
            _converter.UserConverter = value;
        }
    }

    private class converter : IValueConverter
    {
        public IValueConverter? UserConverter { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            value = T.Instance.Convert(value, targetType, parameter, culture);
            if (UserConverter is not null)
                value = UserConverter.Convert(value, targetType, parameter, culture);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (UserConverter is not null)
                value = UserConverter.ConvertBack(value, targetType, parameter, culture);
            value = T.Instance.ConvertBack(value, targetType, parameter, culture);
            return value;
        }
    }
}
