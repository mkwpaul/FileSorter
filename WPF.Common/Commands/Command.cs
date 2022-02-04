using System.Windows.Input;

namespace WPF.Common.Commands
{
    public abstract class Command : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public abstract bool CanExecute(object? parameter);

        public abstract void Execute(object? parameter);

        public void UpdateCanExecute() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}