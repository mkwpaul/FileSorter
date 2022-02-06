using System.Windows;
using System.Windows.Controls;

namespace WPF.Common.Controls
{
    public class LogEntryPresenter : Control
    {
        static LogEntryPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LogEntryPresenter), new FrameworkPropertyMetadata(typeof(LogEntryPresenter)));
        }

        public static readonly DependencyProperty ActionLogProperty = DependencyProperty.Register
        (
            nameof(ActionLog),
            typeof(IActionLog),
            typeof(LogEntryPresenter),
            new FrameworkPropertyMetadata
            {
                DefaultValue = null,
            }
        );

        public IActionLog ActionLog
        {
            get => (IActionLog)GetValue(ActionLogProperty);
            set => SetValue(ActionLogProperty, value);
        }
    }
}
