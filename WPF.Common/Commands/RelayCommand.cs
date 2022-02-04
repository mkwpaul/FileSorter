using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF.Common;

namespace WPF.Common.Commands
{
    public class RelayCommand<T> : Command
    {
        private readonly Action<T> action;
        private readonly Func<T, bool>? predicate;

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
            action((T)parameter!);
        }
    }

    public class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action action, Func<bool> func = null!) : base(x => action(), x => func == null || func())
        {
        }
    }
}
