using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WPF.Common.Controls
{
    public class LogView : Control
    {
        static LogView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LogView), new FrameworkPropertyMetadata(typeof(LogView)));
        }

        public ItemsControl? PART_ItemsPresenter;

        public CollectionViewSource SortedLogs { get; }

        public LogView()
        {
            SortedLogs = new();
            SortedLogs.IsLiveSortingRequested = true;
            var sort = new System.ComponentModel.SortDescription(nameof(IActionLog.Time), System.ComponentModel.ListSortDirection.Descending);
            SortedLogs.SortDescriptions.Add(sort);
        }

        public override void OnApplyTemplate()
        {
            if (Template.FindName("PART_ItemsPresenter", this) is ItemsControl items)
            {
                PART_ItemsPresenter = items;
                if (LogsSource != null)
                {
                    SortedLogs.Source = LogsSource;

                    var binding = new Binding
                    {
                        Source = this,
                        Path = new PropertyPath(nameof(SortedLogs) + "." + nameof(SortedLogs.View)),
                    };
                    items.SetBinding(ItemsControl.ItemsSourceProperty, binding);
                }
            }
        }

        public static readonly DependencyProperty LogsSourceProperty = DependencyProperty.Register
        (
            nameof(LogsSource),
            typeof(object),
            typeof(LogView),
            new FrameworkPropertyMetadata
            {
                DefaultValue = null,
                PropertyChangedCallback = (s, e) => (s as LogView)!.OnLogsSourceChanged(e)
            }
        );

        private void OnLogsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                SortedLogs.Source = e.NewValue;
                if (PART_ItemsPresenter != null)
                {
                    var binding = new Binding
                    {
                        Source = this,
                        Path = new PropertyPath("SortedLogs.View"),
                    };
                    PART_ItemsPresenter.SetBinding(ItemsControl.ItemsSourceProperty, binding);
                }
            }

            if (e.NewValue is INotifyCollectionChanged newVal)
            {
                newVal.CollectionChanged += CollectionNotifier_CollectionChanged;
            }

            if (e.OldValue is INotifyCollectionChanged oldVal)
            {
                oldVal.CollectionChanged -= CollectionNotifier_CollectionChanged;
            }
        }

        private void CollectionNotifier_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SortedLogs.View?.Refresh();
        }

        public object LogsSource
        {
            get => GetValue(LogsSourceProperty);
            set => SetValue(LogsSourceProperty, value);
        }
    }
}
