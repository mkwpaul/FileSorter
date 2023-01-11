using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WPF.Common;

public abstract class PropertyChangedNotifier : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void SetProperty<T>(ref T? t, T? value, [CallerMemberName]string propertyName = null!)
    {
        if (EqualityComparer<T>.Default.Equals(t, value))
            return;

        t = value;
        NotifyPropertyChanged(propertyName);
    }

    public void FowardNotification(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null)
            return;

        NotifyPropertyChanged(e.PropertyName);
    }

    public void ClearNotifications()
    {
        PropertyChanged = null;
    }
}
