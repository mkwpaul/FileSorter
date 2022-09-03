﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using WPF.Common.Converters;

namespace WPF.Common.MarkupExtensions
{
    public class VisibilityBinding : CustomBindingBase
    {
        private readonly converter bindingConverter;
        public Visibility HiddenVisibilty { get; set; } = Visibility.Collapsed;
        public override IValueConverter? Converter
        {
            get => bindingConverter.UserConverter;
            set => bindingConverter.UserConverter = value;
        }

        public bool Invert { get; set; }

        public VisibilityBinding()
        {
            Binding.Converter = bindingConverter = new converter() { Parent =  this };
        }

        public VisibilityBinding(string path) : base(path)
        {
            Binding.Converter = bindingConverter = new converter() { Parent = this };
        }

        private class converter : IValueConverter
        {
            public IValueConverter? UserConverter { get; set; }

            public VisibilityBinding Parent { get; init; }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (UserConverter is not null)
                    value = UserConverter.Convert(value, targetType, parameter, culture);

                return value switch
                {
                    Visibility vis => FromVisibilty(vis),
                    UIElement ui => FromVisibilty(ui.Visibility),
                    bool b => FromBool(b),
                    var obj => FromBool(obj.ToBool()),
                };
            }

            private Visibility FromVisibilty(Visibility vis)
            {
                if (!Parent.Invert)
                    return vis;
                if (vis == Visibility.Visible)
                    return Parent.HiddenVisibilty;
                return Visibility.Visible;
            }

            private Visibility FromBool(bool val)
            {
                return val ^ Parent.Invert ? Visibility.Visible : Parent.HiddenVisibilty;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return Binding.DoNothing;
            }
        }
    }
}