using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BetterMessageBox;

public abstract class PropertyChangedBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void SetProperty<T>(ref T t, T value, [CallerMemberName] string propertyName = null!)
    {
        if (t is null)
        {
            if (value is null)
                return;
        }
        else if (t.Equals(value))
        {
            return;
        }

        t = value;
        NotifyPropertyChanged(propertyName);
    }

    public void FowardNotification(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null)
            return;

        NotifyPropertyChanged(e.PropertyName);
    }
}