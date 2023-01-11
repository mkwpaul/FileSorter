using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WPF.Common.Controls;

[DefaultProperty("Child")]
[ContentProperty("Child")]
public class LabelDecorator : Control
{
    Border? PART_childArea;

    static LabelDecorator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LabelDecorator), new FrameworkPropertyMetadata(typeof(LabelDecorator)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        PART_childArea = Template.FindName(nameof(PART_childArea), this) as Border;

        if (PART_childArea is not null)
            PART_childArea.Child = Child;
    }

    public static readonly DependencyProperty ChildProperty = DependencyProperty.Register
    (
        nameof(Child),
        typeof(UIElement),
        typeof(LabelDecorator),
        new FrameworkPropertyMetadata
        {
            DefaultValue = null,
            PropertyChangedCallback = (o, e) => ((LabelDecorator)o).OnChildChanged(e),
        }
    );

    void OnChildChanged(DependencyPropertyChangedEventArgs e)
    {
        if (PART_childArea is not null)
            PART_childArea.Child = e.NewValue as UIElement;
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register
    (
        nameof(Text),
        typeof(string),
        typeof(LabelDecorator),
        new FrameworkPropertyMetadata
        {
            DefaultValue = "",
        }
    );

    public static readonly DependencyProperty DockProperty = DependencyProperty.Register
    (
        nameof(Dock),
        typeof(Dock),
        typeof(LabelDecorator),
        new FrameworkPropertyMetadata
        {
            DefaultValue = Dock.Top,
        }
    );

    public static readonly DependencyProperty VerticalTextAlignmentProperty = DependencyProperty.Register
    (
        nameof(VerticalTextAlignment),
        typeof(VerticalAlignment),
        typeof(LabelDecorator),
        new FrameworkPropertyMetadata
        {
            DefaultValue = VerticalAlignment.Center,
        }
    );

    public static readonly DependencyProperty HorizontalTextAlignmentProperty = DependencyProperty.Register
    (
        nameof(HorizontalTextAlignment),
        typeof(HorizontalAlignment),
        typeof(LabelDecorator),
        new FrameworkPropertyMetadata
        {
            DefaultValue = HorizontalAlignment.Left,
        }
    );

    public UIElement Child
    {
        get => (UIElement)GetValue(ChildProperty);
        set => SetValue(ChildProperty, value);
    }

    public Dock Dock
    {
        get => (Dock)GetValue(DockProperty);
        set => SetValue(DockProperty, value);
    }

    public VerticalAlignment VerticalTextAlignment
    {
        get => (VerticalAlignment)GetValue(VerticalTextAlignmentProperty);
        set => SetValue(VerticalTextAlignmentProperty, value);
    }

    public HorizontalAlignment HorizontalTextAlignment
    {
        get => (HorizontalAlignment)GetValue(HorizontalTextAlignmentProperty);
        set => SetValue(VerticalTextAlignmentProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}
