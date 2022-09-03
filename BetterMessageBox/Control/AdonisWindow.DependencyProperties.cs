using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace BetterMessageBox.Control;

public partial class AdonisWindow : Window
{
    public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.Register(nameof(IconVisibility), typeof(Visibility), typeof(AdonisWindow), new PropertyMetadata(Visibility.Visible));

    protected internal static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register(nameof(IconSource), typeof(ImageSource), typeof(AdonisWindow), new PropertyMetadata(null));

    protected internal static readonly DependencyPropertyKey TitleBarActualHeightPropertyKey = DependencyProperty.RegisterReadOnly(nameof(TitleBarActualHeight), typeof(double), typeof(AdonisWindow), new PropertyMetadata(0.0d));

    protected internal static readonly DependencyProperty TitleBarActualHeightProperty = TitleBarActualHeightPropertyKey.DependencyProperty;

    public static readonly DependencyProperty TitleBarContentProperty = DependencyProperty.Register(nameof(TitleBarContent), typeof(object), typeof(AdonisWindow), new PropertyMetadata(null));

    public static readonly DependencyProperty TitleBarForegroundProperty = DependencyProperty.Register(nameof(TitleBarForeground), typeof(Brush), typeof(AdonisWindow), new PropertyMetadata(null));

    public static readonly DependencyProperty TitleBarBackgroundProperty = DependencyProperty.Register(nameof(TitleBarBackground), typeof(Brush), typeof(AdonisWindow), new PropertyMetadata(null));

    public static readonly DependencyProperty TitleVisibilityProperty = DependencyProperty.Register(nameof(TitleVisibility), typeof(Visibility), typeof(AdonisWindow), new PropertyMetadata(Visibility.Visible));

    public static readonly DependencyProperty WindowButtonHighlightBrushProperty = DependencyProperty.Register(nameof(WindowButtonHighlightBrush), typeof(Brush), typeof(AdonisWindow), new PropertyMetadata(null));

    protected internal static readonly DependencyPropertyKey MaximizeBorderThicknessPropertyKey = DependencyProperty.RegisterReadOnly(nameof(MaximizeBorderThickness), typeof(Thickness), typeof(AdonisWindow), new PropertyMetadata(new Thickness()));

    protected internal static readonly DependencyProperty MaximizeBorderThicknessProperty = MaximizeBorderThicknessPropertyKey.DependencyProperty;

    public static readonly DependencyProperty ShrinkTitleBarWhenMaximizedProperty = DependencyProperty.Register(nameof(ShrinkTitleBarWhenMaximized), typeof(bool), typeof(AdonisWindow), new PropertyMetadata(true));

    public static readonly DependencyProperty PlaceTitleBarOverContentProperty = DependencyProperty.Register(nameof(PlaceTitleBarOverContent), typeof(bool), typeof(AdonisWindow), new PropertyMetadata(false));
}
