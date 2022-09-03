using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace BetterMessageBox;

public class UINavigator
{
    public static T? FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
    {
        return IterateChildren(depObj).OfType<T>().FirstOrDefault();
    }

    public static T? FindVisualChild<T>(DependencyObject depObj, string name) where T : DependencyObject
    {
        return IterateChildren(depObj).OfType<FrameworkElement>().FirstOrDefault(x => x.Name == name) as T;
    }

    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        return IterateChildren(depObj).OfType<T>();
    }

    public static IEnumerable<DependencyObject> IterateChildren(DependencyObject depObj)
    {
        if (depObj == null)
            yield break;

        var stack = new Stack<DependencyObject>();
        stack.Push(depObj);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            var childrenCount = VisualTreeHelper.GetChildrenCount(current);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(current, i);
                yield return child;

                stack.Push(child);
            }
        }
    }
}
