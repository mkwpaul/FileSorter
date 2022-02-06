using Microsoft.Extensions.Logging;

namespace WPF.Common
{
    public interface IActionLog
    {
        string Description { get; }
        TaskStatus Status { get; }
        LogLevel LogLevel { get; }
        Exception? Exception { get; }

        public TimeOnly Time { get; }
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

        public string Description { get; init; } = "";

        public TaskCompletionNotifier TaskWrapper { get; }

        public TaskLog(Task task)
        {
            Task = task;
            TaskWrapper = new TaskCompletionNotifier(task);
            TaskWrapper.PropertyChanged += TaskWrapper_PropertyChanged;
        }

        private void TaskWrapper_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(TaskWrapper.TaskStatus):
                    NotifyPropertyChanged(nameof(Status));
                    if (Status == TaskStatus.Faulted)
                    {
                        LogLevel = LogLevel.Error;
                        NotifyPropertyChanged(nameof(LogLevel));
                    }

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
}
