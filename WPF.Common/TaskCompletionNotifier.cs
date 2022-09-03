namespace WPF.Common;

public class TaskCompletionNotifier : PropertyChangedNotifier
{
    public TaskCompletionNotifier(Task task)
    {
        Task = task;
        if (task.IsCompleted)
        {
            return;
        }

        var scheduler = (SynchronizationContext.Current == null) ? TaskScheduler.Current : TaskScheduler.FromCurrentSynchronizationContext();
        Task.ContinueWith(UpdateStatus, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, scheduler);
    }

    void UpdateStatus(Task t)
    {
        NotifyPropertyChanged(nameof(IsCompleted));
        NotifyPropertyChanged(nameof(TaskStatus));
        switch (t.Status)
        {
            case TaskStatus.RanToCompletion:
                NotifyPropertyChanged(nameof(IsSuccessfullyCompleted));
                break;
            case TaskStatus.Canceled:
                NotifyPropertyChanged(nameof(IsCanceled));
                break;
            case TaskStatus.Faulted:
                NotifyPropertyChanged(nameof(IsFaulted));
                NotifyPropertyChanged(nameof(Exception));
                break;
        }
    }

    public TaskStatus TaskStatus => Task.Status;

    public virtual Task Task { get; }
    public bool IsCompleted => Task.IsCompleted;
    public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;
    public bool IsCanceled => Task.IsCanceled;
    public bool IsFaulted => Task.IsFaulted;
    public Exception? Exception => Task.Exception?.InnerException;
}

/// <summary>
/// Watches a task and raises property-changed notifications when the task completes.
/// </summary>
public sealed class TaskCompletionNotifier<TResult> : TaskCompletionNotifier
{
    public TaskCompletionNotifier(Task<TResult> task) : base(task)
    {
        Task = task;
        if (task.IsCompleted)
            return;

        var scheduler = (SynchronizationContext.Current == null) ? TaskScheduler.Current : TaskScheduler.FromCurrentSynchronizationContext();
        Task.ContinueWith(UpdateStatus, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, scheduler);
    }

    private void UpdateStatus(Task<TResult> t)
    {
        switch (t.Status)
        {
            case TaskStatus.RanToCompletion:
                NotifyPropertyChanged(nameof(Result));
                break;
        }
    }

    public override Task<TResult> Task { get; }
    public TResult? Result => (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : default;
}
