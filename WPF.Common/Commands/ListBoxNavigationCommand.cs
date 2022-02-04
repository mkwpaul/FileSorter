using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WPF.Common.Commands
{
    public enum NavigationDirection
    {
        None,
        Next,
        Previous,
        First,
        Last,
    }

    public class ListBoxNavigationCommand : Command
    {
        public bool WrapNavigation { get; set; } = true;
        public NavigationDirection Direction { get; set; } 

        public ListBox? ListBox { get; set; }

        public override bool CanExecute(object? parameter)
        {
            return true;
        }

        public override void Execute(object? parameter)
        {
            if (ListBox is null)
                return;
            switch (Direction)
            {
                case NavigationDirection.Next: NavigateNext(ListBox); break;
                case NavigationDirection.Previous: NavigatePrevious(ListBox); break;
                case NavigationDirection.First: NavigateFirst(ListBox); break;
                case NavigationDirection.Last: NavigateLast(ListBox); break;
            }
            ListBox.ScrollIntoView(ListBox.SelectedItem);
        }

        private static void NavigateFirst(ListBox listbox)
        {
            listbox.SelectedIndex = 0;
        }

        private static void NavigateLast(ListBox listbox)
        {
            listbox.SelectedIndex = listbox.Items.Count - 1;
        }

        private void NavigateNext(ListBox listbox)
        {
            int nextIndex = (listbox.SelectedIndex + 1).Upperlimit(listbox.Items.Count);

            if (nextIndex == listbox.Items.Count)
            {
                if (WrapNavigation)
                    nextIndex = 0;
                else
                    return;
            }
            listbox.SelectedIndex = nextIndex;
        }

        private void NavigatePrevious(ListBox listbox)
        {
            int nextIndex = (listbox.SelectedIndex - 1).LowerLimit(-1);

            if (nextIndex == -1)
            {
                if (WrapNavigation)
                    nextIndex = listbox.Items.Count - 1;
                else
                    return;
            }
            listbox.SelectedIndex = nextIndex;
        }
    }
}
