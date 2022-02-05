using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF.Common.Commands;

namespace WPF.Common.Controls
{
    public interface IActionLog
    {
        string Description { get; }
        TaskStatus Status { get; }
        LogLevel LogLevel { get; }
        Exception? Exception { get; }

        public TimeOnly Time { get; }
    }

    public static class ActionLogModule
    {
        public static IActionLog Log(this Task task, string description = "", LogLevel logLevel = LogLevel.Information)
        {
            return new TaskLog(task) { Description = description, LogLevel = logLevel };
        }

        public static IActionLog LogSuccess(this string description, LogLevel logLevel = LogLevel.Information)
        {
            return new SuccessLog(description) { LogLevel = logLevel };
        }

        public static IActionLog Log(this Exception exception, string descrpition = "")
        {
            return new ExceptionLog(exception) { Description = descrpition };
        }

        public static TimeOnly GetTime(this DateTime dateTIme) 
        {
            return new TimeOnly(dateTIme.TimeOfDay.Ticks);
        }

        public static void Log(this ICollection<IActionLog> logs, string description, LogLevel logLevel = LogLevel.Information)
        {
            var log = description.LogSuccess(logLevel);
            logs.Add(log);
        }
    }

    public record ExceptionLog(Exception Exception) : IActionLog
    {
        public string Description { get; init; } = "";

        public TaskStatus Status => TaskStatus.Faulted;

        public LogLevel LogLevel => LogLevel.Error;

        public TimeOnly Time { get; } = DateTime.Now.GetTime();
    }

    public record SuccessLog(string Description) : IActionLog
    {
        public TaskStatus Status => TaskStatus.RanToCompletion;

        public LogLevel LogLevel { get; init; }

        public Exception? Exception => null;

        public TimeOnly Time { get; } = DateTime.Now.GetTime();
    }

    public class TaskLog : PropertyChangedNotifier, IActionLog
    {
        public Task Task { get; }

        public string Description { get; init; }

        public TaskCompletionNotifier TaskWrapper { get; }

        public TaskLog(Task task)
        {
            Task = task;
            TaskWrapper = new TaskCompletionNotifier(task);
            TaskWrapper.PropertyChanged += TaskWrapper_PropertyChanged; ;
        }

        private void TaskWrapper_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(TaskWrapper.TaskStatus):
                    NotifyPropertyChanged(nameof(Status));
                    break;
                case nameof(TaskWrapper.Exception):
                    NotifyPropertyChanged(nameof(Exception));
                    break;
                default:
                    break;
            }
        }

        public TaskStatus Status => TaskWrapper.TaskStatus;

        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        public Exception? Exception => TaskWrapper.Task.Exception;

        public TimeOnly Time { get; } = DateTime.Now.GetTime();
    }

    public class ActionLogPresenter : Control
    {
        static ActionLogPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ActionLogPresenter), new FrameworkPropertyMetadata(typeof(ActionLogPresenter)));
        }

        public static readonly DependencyProperty ActionLogProperty = DependencyProperty.Register
        (
            nameof(ActionLog),
            typeof(IActionLog),
            typeof(ActionLogPresenter),
            new FrameworkPropertyMetadata
            {
                DefaultValue = null,
                PropertyChangedCallback = (s, e) => (s as ActionLogPresenter).OnActionLogChanged(e)
            }
        );

        private void OnActionLogChanged(DependencyPropertyChangedEventArgs e)
        {

            
        }

        public IActionLog ActionLog
        {
            get => (IActionLog)GetValue(ActionLogProperty);
            set => SetValue(ActionLogProperty, value);
        }

        // private static readonly DependencyPropertyKey TimePropertyKey = DependencyProperty.RegisterReadOnly
        // (
        //     nameof(Time),
        //     typeof(TimeOnly),
        //     typeof(ActionLogPresenter),
        //     new FrameworkPropertyMetadata
        //     {
        //         DefaultValue = TimeOnly.MinValue
        //     }
        // );
        // 
        // public static readonly DependencyProperty TimeProperty = TimePropertyKey.DependencyProperty;
        // public TimeOnly Time
        // {
        //     get => (TimeOnly)GetValue(TimeProperty);
        //     private set => SetValue(TimePropertyKey, value);
        // }
        // 
        // private static readonly DependencyPropertyKey StatusPropertyKey = DependencyProperty.RegisterReadOnly
        // (
        //     nameof(Status),
        //     typeof(TaskStatus),
        //     typeof(ActionLogPresenter),
        //     new FrameworkPropertyMetadata
        //     {
        //         DefaultValue = TaskStatus.RanToCompletion,
        //     }
        // );
        // public static readonly DependencyProperty StatusProperty = StatusPropertyKey.DependencyProperty;
        // 
        // public TaskStatus Status
        // {
        //     get => (TaskStatus)GetValue(StatusProperty);
        //     private set => SetValue(StatusPropertyKey, value);
        // }
    }

    public class ActionLogsCollectionPresenter : Control
    {
        static ActionLogsCollectionPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ActionLogsCollectionPresenter), new FrameworkPropertyMetadata(typeof(ActionLogsCollectionPresenter)));
        }

        public ItemsControl? PART_ItemsControl;

        public override void OnApplyTemplate()
        {
            if (Template.FindName("PART_ItemsPresenter", this) is ItemsControl items)
            {
                PART_ItemsControl = items;
                if (LogsSource != null)
                {
                    SortedLogs.Source = LogsSource;

                    var binding = new Binding
                    {
                        Source = this,
                        Path = new PropertyPath("SortedLogs.View"),
                    };
                    items.SetBinding(ItemsControl.ItemsSourceProperty, binding);
                }
            }
        }

        public static readonly DependencyProperty LogsSourceProperty = DependencyProperty.Register
        (
            nameof(LogsSource),
            typeof(object),
            typeof(ActionLogsCollectionPresenter),
            new FrameworkPropertyMetadata
            {
                DefaultValue = null,
                PropertyChangedCallback = (s, e) => (s as ActionLogsCollectionPresenter).OnLogsSourceChanged(e)
            }
        );

        public CollectionViewSource SortedLogs { get; } = new();

        private void OnLogsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                SortedLogs.Source = e.NewValue;
                if (PART_ItemsControl != null)
                {

                    var binding = new Binding
                    {
                        Source = this,
                        Path = new PropertyPath("SortedLogs.View"),
                    };
                    PART_ItemsControl.SetBinding(ItemsControl.ItemsSourceProperty, binding);
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
