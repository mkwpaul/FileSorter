using Serilog;
using System.Windows.Input;
namespace WPF.Common.Commands;

public abstract class Command : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public abstract bool CanExecute(object? parameter);

    public abstract void Execute(object? parameter);

    public void UpdateCanExecute() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public class RelayCommand<T> : Command
{
    static readonly ILogger _log = Log.Logger.ForContext<RelayCommand>();
    readonly Action<T> action;
    readonly Func<T, bool>? predicate;

    public RelayCommand(Action<T> action, Func<T, bool>? predicate = null)
    {
        this.action = action ?? throw new ArgumentException(null, nameof(action));
        this.predicate = predicate;
    }

    public override bool CanExecute(object? parameter)
    {
        if (predicate == null)
            return true;
        return predicate((T)parameter!);
    }

    public override void Execute(object? parameter)
    {
        try
        {
            action((T)parameter!);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Command Error. Parameter: {parameter}; Type: {type}", parameter, typeof(T));
            throw;
        }
    }
}

public class RelayCommand : RelayCommand<object>
{
    public RelayCommand(Action action, Func<bool> func = null!) : base(_ => action(), _ => func == null || func())
    {
    }
}

public class AsyncCommand<T> : Command
{
    readonly Func<T, Task> action;
    readonly Func<T, bool>? predicate;

    static readonly ILogger _log = Log.Logger.ForContext<AsyncCommand>();

    public AsyncCommand(Func<T, Task> action, Func<T, bool>? predicate = null)
    {
        this.action = action ?? throw new ArgumentException(null, nameof(action));
        this.predicate = predicate;
    }

    public override bool CanExecute(object? parameter)
    {
        if (predicate == null)
            return true;
        return predicate((T)parameter!);
    }

    public override async void Execute(object? parameter)
    {
        try
        {
            await action((T)parameter!);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Command Error. Parameter: {parameter}; Type: {type}", parameter, typeof(T));
            throw;
        }
    }
}

public class AsyncCommand : AsyncCommand<object>
{
    public AsyncCommand(Func<Task> action, Func<bool> func = null!) : base(_ => action(), _ => func == null || func())
    {
    }
}