using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WPF.Common
{
    public abstract class PropertyChangedNotifier : INotifyPropertyChanged
    {
        public Type Type { get; }
        public HashSet<string> PropertyNames { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly PropertyInfo[] propertyInfos;

        protected PropertyChangedNotifier()
        {
            Type = GetType();
            propertyInfos = Type.GetProperties();
            PropertyNames = propertyInfos.Select(x => x.Name).ToHashSet();
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetProperty<T>(ref T t, T value, [CallerMemberName]string propertyName = null!)
        {
            if (t is null)
            {
                if (value is null)
                    return;
            }
            else if (t.Equals(value))
                return;

            t = value;
            NotifyPropertyChanged(propertyName);
        }

        public void FowardNotification(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is null)
                return;

            if (PropertyNames.Contains(e.PropertyName))
                NotifyPropertyChanged(e.PropertyName);
        }
    }
}
